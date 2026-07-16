using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.AdminDashboard.GetAdminDashboardSummary;

public sealed class GetAdminDashboardSummaryQueryHandler(
    IBusinessRepository businessRepository,
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider) : IQueryHandler<GetAdminDashboardSummaryQuery, GetAdminDashboardSummaryResult>
{
    private const int DefaultRangeDays = 7;
    private const int MaxDateRangeDays = 90;
    private const int UpcomingAppointmentLimit = 5;

    public async Task<GetAdminDashboardSummaryResult> HandleAsync(GetAdminDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(query.BusinessId, cancellationToken);
        if (business is null)
        {
            return GetAdminDashboardSummaryResult.Failure(GetAdminDashboardSummaryError.BusinessNotFound);
        }

        if (!timeZoneProvider.TryGetIanaTimeZoneInfo(business.TimeZoneId, out var businessTimeZone))
        {
            return GetAdminDashboardSummaryResult.Failure(GetAdminDashboardSummaryError.InvalidBusinessTimeZone);
        }

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, businessTimeZone).DateTime);
        var fromLocalDate = query.FromLocalDate ?? today;
        var toLocalDate = query.ToLocalDate ?? today.AddDays(DefaultRangeDays);

        if (toLocalDate < fromLocalDate)
        {
            return GetAdminDashboardSummaryResult.Failure(GetAdminDashboardSummaryError.InvalidDateRange);
        }

        if (toLocalDate.DayNumber - fromLocalDate.DayNumber > MaxDateRangeDays)
        {
            return GetAdminDashboardSummaryResult.Failure(GetAdminDashboardSummaryError.DateRangeTooLarge);
        }

        var rangeStartUtc = ToUtc(fromLocalDate.ToDateTime(TimeOnly.MinValue), businessTimeZone);
        var rangeEndUtc = ToUtc(toLocalDate.AddDays(1).ToDateTime(TimeOnly.MinValue), businessTimeZone);
        var todayStartUtc = ToUtc(today.ToDateTime(TimeOnly.MinValue), businessTimeZone);
        var todayEndUtc = ToUtc(today.AddDays(1).ToDateTime(TimeOnly.MinValue), businessTimeZone);
        var nowUtc = DateTimeOffset.UtcNow;

        var rangeAppointments = await appointmentRepository.ListByBusinessIdWithDetailsAsync(
            query.BusinessId,
            rangeStartUtc,
            rangeEndUtc,
            staffMemberId: null,
            serviceId: null,
            status: null,
            cancellationToken);
        var todayAppointments = await appointmentRepository.ListByBusinessIdWithDetailsAsync(
            query.BusinessId,
            todayStartUtc,
            todayEndUtc,
            staffMemberId: null,
            serviceId: null,
            status: null,
            cancellationToken);

        var upcomingAppointments = rangeAppointments
            .Where(appointment => appointment.StartAtUtc >= nowUtc)
            .Where(appointment => appointment.Status is not (AppointmentStatus.CancelledByAdmin or AppointmentStatus.CancelledByCustomer))
            .OrderBy(appointment => appointment.StartAtUtc)
            .Take(UpcomingAppointmentLimit)
            .Select(appointment => AppointmentDetailsMapper.Map(appointment, timeZoneProvider))
            .ToList();
        var estimatedRevenue = rangeAppointments
            .Where(appointment => appointment.Status is not (AppointmentStatus.CancelledByAdmin or AppointmentStatus.CancelledByCustomer))
            .Sum(appointment => appointment.PriceAmountSnapshot);
        var statusCounts = rangeAppointments
            .GroupBy(appointment => appointment.Status)
            .OrderBy(group => group.Key.ToString())
            .Select(group => new AdminDashboardStatusCount(group.Key.ToString(), group.Count()))
            .ToList();

        return GetAdminDashboardSummaryResult.Success(new AdminDashboardSummary(
            todayAppointments.Count,
            upcomingAppointments,
            estimatedRevenue,
            business.CurrencyCode,
            statusCounts,
            fromLocalDate,
            toLocalDate));
    }

    private static DateTimeOffset ToUtc(DateTime localDateTime, TimeZoneInfo timeZone) =>
        new(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified), timeZone), TimeSpan.Zero);
}
