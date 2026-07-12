namespace Calendar.Application.Services;

public sealed record AdminServiceDetails(
    Guid Id,
    Guid BusinessId,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal PriceAmount,
    bool IsActive,
    int SortOrder,
    DateTimeOffset CreatedAtUtc);

public sealed record ListAdminServicesResult(
    bool BusinessFound,
    IReadOnlyList<AdminServiceDetails> Services)
{
    public static ListAdminServicesResult Success(IReadOnlyList<AdminServiceDetails> services) => new(true, services);

    public static ListAdminServicesResult NotFound() => new(false, []);
}

public enum GetAdminServiceError
{
    None = 0,
    BusinessNotFound = 1,
    ServiceNotFound = 2
}

public sealed record GetAdminServiceResult(
    bool Succeeded,
    GetAdminServiceError Error,
    AdminServiceDetails? Service)
{
    public static GetAdminServiceResult Success(AdminServiceDetails service) => new(
        true,
        GetAdminServiceError.None,
        service);

    public static GetAdminServiceResult Failure(GetAdminServiceError error) => new(
        false,
        error,
        null);
}
