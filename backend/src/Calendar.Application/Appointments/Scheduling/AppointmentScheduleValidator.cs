using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.Scheduling;

public sealed class AppointmentScheduleValidator(
    IBusinessRepository businessRepository,
    IServiceRepository serviceRepository,
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberServiceRepository staffMemberServiceRepository,
    IStaffMemberAvailabilityRepository availabilityRepository,
    IStaffMemberAvailabilityExceptionRepository availabilityExceptionRepository,
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider) : IAppointmentScheduleValidator
{
    public async Task<AppointmentScheduleValidationResult> ValidateAsync(
        AppointmentScheduleValidationRequest request,
        CancellationToken cancellationToken)
    {
        var startAtUtc = request.StartAtUtc.ToUniversalTime();
        var endAtUtc = request.EndAtUtc.ToUniversalTime();

        if (startAtUtc >= endAtUtc)
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.InvalidTimeRange);
        }

        var business = await businessRepository.GetActiveByIdAsync(request.BusinessId, cancellationToken);
        if (business is null)
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.BusinessNotFound);
        }

        if (!timeZoneProvider.TryGetIanaTimeZoneInfo(business.TimeZoneId, out var businessTimeZone))
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.InvalidBusinessTimeZone);
        }

        var localStart = TimeZoneInfo.ConvertTime(startAtUtc, businessTimeZone);
        var localEnd = TimeZoneInfo.ConvertTime(endAtUtc, businessTimeZone);
        var localDate = DateOnly.FromDateTime(localStart.DateTime);

        if (localDate != DateOnly.FromDateTime(localEnd.DateTime))
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.OutsideAvailability);
        }

        if (!IsDateWithinBookingWindow(localDate, business, businessTimeZone))
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.OutsideBookingWindow);
        }

        var service = await serviceRepository.GetActiveByIdAndBusinessIdAsync(
            request.ServiceId,
            request.BusinessId,
            cancellationToken);

        if (service is null)
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.ServiceNotFound);
        }

        var staffMember = await staffMemberRepository.GetActiveByIdAndBusinessIdAsync(
            request.StaffMemberId,
            request.BusinessId,
            cancellationToken);

        if (staffMember is null)
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.StaffMemberNotFound);
        }

        var assignmentExists = await staffMemberServiceRepository.ExistsActiveAsync(
            request.StaffMemberId,
            request.ServiceId,
            cancellationToken);

        if (!assignmentExists)
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.StaffMemberServiceAssignmentNotFound);
        }

        var ranges = await GetAvailabilityRangesAsync(
            request.StaffMemberId,
            localDate,
            cancellationToken);

        var localStartTime = TimeOnly.FromDateTime(localStart.DateTime);
        var localEndTime = TimeOnly.FromDateTime(localEnd.DateTime);

        if (!ranges.Any(range => range.StartTime <= localStartTime && localEndTime <= range.EndTime))
        {
            return AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.OutsideAvailability);
        }

        var overlaps = await appointmentRepository.HasBlockingOverlapAsync(
            request.BusinessId,
            request.StaffMemberId,
            startAtUtc,
            endAtUtc,
            request.ExcludedAppointmentId,
            cancellationToken);

        return overlaps
            ? AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.AppointmentOverlaps)
            : AppointmentScheduleValidationResult.Success();
    }

    private async Task<IReadOnlyList<AvailabilityRange>> GetAvailabilityRangesAsync(
        Guid staffMemberId,
        DateOnly localDate,
        CancellationToken cancellationToken)
    {
        var staffMemberIds = new[] { staffMemberId };
        var exceptions = await availabilityExceptionRepository.ListByStaffMemberIdsAndDateAsync(
            staffMemberIds,
            localDate,
            cancellationToken);

        if (exceptions.Any(exception => exception.IsClosed))
        {
            return [];
        }

        if (exceptions.Count > 0)
        {
            return exceptions
                .Where(exception => !exception.IsClosed && exception.StartTime.HasValue && exception.EndTime.HasValue)
                .Select(exception => new AvailabilityRange(exception.StartTime!.Value, exception.EndTime!.Value))
                .ToList();
        }

        var availabilities = await availabilityRepository.ListActiveByStaffMemberIdsAndDayAsync(
            staffMemberIds,
            localDate.DayOfWeek,
            cancellationToken);

        return availabilities
            .Select(availability => new AvailabilityRange(availability.StartTime, availability.EndTime))
            .ToList();
    }

    private static bool IsDateWithinBookingWindow(DateOnly localDate, Business business, TimeZoneInfo businessTimeZone)
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, businessTimeZone).DateTime);
        return localDate >= today && localDate <= today.AddDays(business.MaxAdvanceBookingDays);
    }

    private sealed record AvailabilityRange(TimeOnly StartTime, TimeOnly EndTime);
}
