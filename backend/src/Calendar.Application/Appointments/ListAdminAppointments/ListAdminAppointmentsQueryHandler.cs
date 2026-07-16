using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Appointments.ListAdminAppointments;

public sealed class ListAdminAppointmentsQueryHandler(
    IBusinessRepository businessRepository,
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider) : IQueryHandler<ListAdminAppointmentsQuery, ListAdminAppointmentsResult>
{
    private const int MaxDateRangeDays = 90;

    public async Task<ListAdminAppointmentsResult> HandleAsync(ListAdminAppointmentsQuery query, CancellationToken cancellationToken)
    {
        if (query.ToLocalDate < query.FromLocalDate)
        {
            return ListAdminAppointmentsResult.Failure(ListAdminAppointmentsError.InvalidDateRange);
        }

        if (query.ToLocalDate.DayNumber - query.FromLocalDate.DayNumber > MaxDateRangeDays)
        {
            return ListAdminAppointmentsResult.Failure(ListAdminAppointmentsError.DateRangeTooLarge);
        }

        var business = await businessRepository.GetByIdAsync(query.BusinessId, cancellationToken);
        if (business is null)
        {
            return ListAdminAppointmentsResult.Failure(ListAdminAppointmentsError.BusinessNotFound);
        }

        if (!timeZoneProvider.TryGetIanaTimeZoneInfo(business.TimeZoneId, out var businessTimeZone))
        {
            return ListAdminAppointmentsResult.Failure(ListAdminAppointmentsError.InvalidBusinessTimeZone);
        }

        var fromUtc = ToUtc(query.FromLocalDate.ToDateTime(TimeOnly.MinValue), businessTimeZone);
        var toUtc = ToUtc(query.ToLocalDate.AddDays(1).ToDateTime(TimeOnly.MinValue), businessTimeZone);

        var appointments = await appointmentRepository.ListByBusinessIdWithDetailsAsync(
            query.BusinessId,
            fromUtc,
            toUtc,
            query.StaffMemberId,
            query.ServiceId,
            query.Status,
            cancellationToken);

        return ListAdminAppointmentsResult.Success(
            appointments.Select(appointment => AppointmentDetailsMapper.Map(appointment, timeZoneProvider)).ToList());
    }

    private static DateTimeOffset ToUtc(DateTime localDateTime, TimeZoneInfo timeZone) =>
        new(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified), timeZone), TimeSpan.Zero);
}
