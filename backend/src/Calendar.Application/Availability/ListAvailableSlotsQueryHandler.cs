using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Availability;

public sealed class ListAvailableSlotsQueryHandler(
    IBusinessRepository businessRepository,
    IServiceRepository serviceRepository,
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberServiceRepository staffMemberServiceRepository,
    IStaffMemberAvailabilityRepository availabilityRepository,
    IStaffMemberAvailabilityExceptionRepository availabilityExceptionRepository,
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider) : IQueryHandler<ListAvailableSlotsQuery, ListAvailableSlotsResult>
{
    private static readonly TimeSpan SlotStep = TimeSpan.FromMinutes(15);

    public async Task<ListAvailableSlotsResult> HandleAsync(
        ListAvailableSlotsQuery query,
        CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetActiveByIdAsync(query.BusinessId, cancellationToken);
        if (business is null)
        {
            return ListAvailableSlotsResult.Failure(ListAvailableSlotsError.BusinessNotFound);
        }

        if (!timeZoneProvider.TryGetIanaTimeZoneInfo(business.TimeZoneId, out var businessTimeZone))
        {
            return ListAvailableSlotsResult.Failure(ListAvailableSlotsError.InvalidBusinessTimeZone);
        }

        if (!IsDateWithinBookingWindow(query.LocalDate, business, businessTimeZone))
        {
            return ListAvailableSlotsResult.Failure(ListAvailableSlotsError.InvalidDate);
        }

        var service = await serviceRepository.GetActiveByIdAndBusinessIdAsync(
            query.ServiceId,
            query.BusinessId,
            cancellationToken);

        if (service is null)
        {
            return ListAvailableSlotsResult.Failure(ListAvailableSlotsError.ServiceNotFound);
        }

        var staffMemberIds = await GetStaffMemberIdsAsync(query, cancellationToken);
        if (staffMemberIds is null)
        {
            return query.StaffMemberId.HasValue
                ? ListAvailableSlotsResult.Failure(ListAvailableSlotsError.StaffMemberNotFound)
                : ListAvailableSlotsResult.Success([]);
        }

        if (query.StaffMemberId.HasValue && staffMemberIds.Count == 0)
        {
            return ListAvailableSlotsResult.Failure(ListAvailableSlotsError.StaffMemberServiceAssignmentNotFound);
        }

        if (staffMemberIds.Count == 0)
        {
            return ListAvailableSlotsResult.Success([]);
        }

        if (!TryConvertLocalToUtc(query.LocalDate.ToDateTime(TimeOnly.MinValue), businessTimeZone, out var dayStartUtc)
            || !TryConvertLocalToUtc(query.LocalDate.AddDays(1).ToDateTime(TimeOnly.MinValue), businessTimeZone, out var dayEndUtc))
        {
            return ListAvailableSlotsResult.Failure(ListAvailableSlotsError.InvalidDate);
        }

        var availabilities = await availabilityRepository.ListActiveByStaffMemberIdsAndDayAsync(
            staffMemberIds,
            query.LocalDate.DayOfWeek,
            cancellationToken);

        var exceptions = await availabilityExceptionRepository.ListByStaffMemberIdsAndDateAsync(
            staffMemberIds,
            query.LocalDate,
            cancellationToken);

        var appointments = await appointmentRepository.ListBlockingAppointmentsAsync(
            query.BusinessId,
            staffMemberIds,
            dayStartUtc,
            dayEndUtc,
            cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        var slots = new List<AvailableSlotDetails>();

        foreach (var staffMemberId in staffMemberIds)
        {
            var ranges = GetAvailabilityRanges(staffMemberId, availabilities, exceptions);
            var staffAppointments = appointments
                .Where(appointment => appointment.StaffMemberId == staffMemberId)
                .ToList();

            foreach (var range in ranges)
            {
                AddSlotsForRange(
                    slots,
                    staffMemberId,
                    query.LocalDate,
                    range,
                    service.DurationMinutes,
                    businessTimeZone,
                    staffAppointments,
                    nowUtc);
            }
        }

        return ListAvailableSlotsResult.Success(
            slots
                .OrderBy(slot => slot.StartAtUtc)
                .ThenBy(slot => slot.StaffMemberId)
                .ToList());
    }

    private async Task<IReadOnlyList<Guid>?> GetStaffMemberIdsAsync(
        ListAvailableSlotsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.StaffMemberId.HasValue)
        {
            var staffMember = await staffMemberRepository.GetActiveByIdAndBusinessIdAsync(
                query.StaffMemberId.Value,
                query.BusinessId,
                cancellationToken);

            if (staffMember is null)
            {
                return null;
            }

            var assignmentExists = await staffMemberServiceRepository.ExistsActiveAsync(
                query.StaffMemberId.Value,
                query.ServiceId,
                cancellationToken);

            return assignmentExists ? [query.StaffMemberId.Value] : [];
        }

        var assignments = await staffMemberServiceRepository.ListActiveByBusinessIdAndServiceIdAsync(
            query.BusinessId,
            query.ServiceId,
            cancellationToken);

        return assignments
            .Select(assignment => assignment.StaffMemberId)
            .Distinct()
            .ToList();
    }

    private static bool IsDateWithinBookingWindow(DateOnly localDate, Business business, TimeZoneInfo businessTimeZone)
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, businessTimeZone).DateTime);
        return localDate >= today && localDate <= today.AddDays(business.MaxAdvanceBookingDays);
    }

    private static IReadOnlyList<AvailabilityRange> GetAvailabilityRanges(
        Guid staffMemberId,
        IReadOnlyList<StaffMemberAvailability> availabilities,
        IReadOnlyList<StaffMemberAvailabilityException> exceptions)
    {
        var staffExceptions = exceptions
            .Where(exception => exception.StaffMemberId == staffMemberId)
            .ToList();

        if (staffExceptions.Any(exception => exception.IsClosed))
        {
            return [];
        }

        if (staffExceptions.Count > 0)
        {
            return staffExceptions
                .Where(exception => !exception.IsClosed && exception.StartTime.HasValue && exception.EndTime.HasValue)
                .Select(exception => new AvailabilityRange(exception.StartTime!.Value, exception.EndTime!.Value))
                .ToList();
        }

        return availabilities
            .Where(availability => availability.StaffMemberId == staffMemberId)
            .Select(availability => new AvailabilityRange(availability.StartTime, availability.EndTime))
            .ToList();
    }

    private static void AddSlotsForRange(
        List<AvailableSlotDetails> slots,
        Guid staffMemberId,
        DateOnly localDate,
        AvailabilityRange range,
        int serviceDurationMinutes,
        TimeZoneInfo businessTimeZone,
        IReadOnlyList<Appointment> appointments,
        DateTimeOffset nowUtc)
    {
        var duration = TimeSpan.FromMinutes(serviceDurationMinutes);
        var rangeStart = localDate.ToDateTime(range.StartTime, DateTimeKind.Unspecified);
        var rangeEnd = localDate.ToDateTime(range.EndTime, DateTimeKind.Unspecified);

        for (var slotStart = rangeStart; slotStart + duration <= rangeEnd; slotStart += SlotStep)
        {
            var slotEnd = slotStart + duration;
            if (!TryConvertLocalToUtc(slotStart, businessTimeZone, out var slotStartUtc)
                || !TryConvertLocalToUtc(slotEnd, businessTimeZone, out var slotEndUtc)
                || slotStartUtc < nowUtc
                || appointments.Any(appointment => appointment.StartAtUtc < slotEndUtc && slotStartUtc < appointment.EndAtUtc))
            {
                continue;
            }

            slots.Add(new AvailableSlotDetails(
                staffMemberId,
                localDate,
                TimeOnly.FromDateTime(slotStart),
                TimeOnly.FromDateTime(slotEnd),
                slotStartUtc,
                slotEndUtc));
        }
    }

    private static bool TryConvertLocalToUtc(
        DateTime localDateTime,
        TimeZoneInfo timeZone,
        out DateTimeOffset utcDateTime)
    {
        utcDateTime = default;

        if (timeZone.IsInvalidTime(localDateTime))
        {
            return false;
        }

        utcDateTime = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone), TimeSpan.Zero);
        return true;
    }

    private sealed record AvailabilityRange(TimeOnly StartTime, TimeOnly EndTime);
}
