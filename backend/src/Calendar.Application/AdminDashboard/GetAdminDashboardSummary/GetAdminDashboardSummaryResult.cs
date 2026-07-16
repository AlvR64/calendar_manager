namespace Calendar.Application.AdminDashboard.GetAdminDashboardSummary;

public sealed class GetAdminDashboardSummaryResult
{
    private GetAdminDashboardSummaryResult(bool succeeded, AdminDashboardSummary? summary, GetAdminDashboardSummaryError? error)
    {
        Succeeded = succeeded;
        Summary = summary;
        Error = error;
    }

    public bool Succeeded { get; }

    public AdminDashboardSummary? Summary { get; }

    public GetAdminDashboardSummaryError? Error { get; }

    public static GetAdminDashboardSummaryResult Success(AdminDashboardSummary summary) => new(true, summary, null);

    public static GetAdminDashboardSummaryResult Failure(GetAdminDashboardSummaryError error) => new(false, null, error);
}
