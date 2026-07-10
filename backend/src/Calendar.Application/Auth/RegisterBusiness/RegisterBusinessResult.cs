namespace Calendar.Application.Auth.RegisterBusiness;

public sealed record RegisterBusinessResult(
    bool Succeeded,
    RegisterBusinessError Error,
    Guid? BusinessId,
    string? BusinessSlug,
    Guid? AdminId,
    string? AdminEmail,
    DateTimeOffset? CreatedAtUtc)
{
    public static RegisterBusinessResult Success(
        Guid businessId,
        string businessSlug,
        Guid adminId,
        string adminEmail,
        DateTimeOffset createdAtUtc) => new(
            true,
            RegisterBusinessError.None,
            businessId,
            businessSlug,
            adminId,
            adminEmail,
            createdAtUtc);

    public static RegisterBusinessResult Failure(RegisterBusinessError error) => new(
        false,
        error,
        null,
        null,
        null,
        null,
        null);
}
