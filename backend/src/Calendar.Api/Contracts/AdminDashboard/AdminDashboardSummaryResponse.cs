using Calendar.Api.Contracts.Appointments;

namespace Calendar.Api.Contracts.AdminDashboard;

/// <summary>Represents the admin dashboard appointment summary.</summary>
public sealed record AdminDashboardSummaryResponse(
    int TodayAppointmentCount,
    IReadOnlyList<AppointmentSummaryResponse> UpcomingAppointments,
    decimal EstimatedRevenueAmount,
    string CurrencyCode,
    IReadOnlyList<AdminDashboardStatusCountResponse> StatusCounts,
    DateOnly RangeStartLocalDate,
    DateOnly RangeEndLocalDate);

/// <summary>Represents an appointment count for one status.</summary>
public sealed record AdminDashboardStatusCountResponse(string Status, int Count);
