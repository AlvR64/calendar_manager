using Calendar.Application.Appointments.GetAppointmentDetails;

namespace Calendar.Application.AdminDashboard.GetAdminDashboardSummary;

public sealed record AdminDashboardSummary(
    int TodayAppointmentCount,
    IReadOnlyList<AppointmentDetails> UpcomingAppointments,
    decimal EstimatedRevenueAmount,
    string CurrencyCode,
    IReadOnlyList<AdminDashboardStatusCount> StatusCounts,
    DateOnly RangeStartLocalDate,
    DateOnly RangeEndLocalDate);

public sealed record AdminDashboardStatusCount(string Status, int Count);
