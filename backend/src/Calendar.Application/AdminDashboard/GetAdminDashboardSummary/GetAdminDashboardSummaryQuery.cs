using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.AdminDashboard.GetAdminDashboardSummary;

public sealed record GetAdminDashboardSummaryQuery(
    Guid BusinessId,
    DateOnly? FromLocalDate,
    DateOnly? ToLocalDate) : IQuery<GetAdminDashboardSummaryResult>;
