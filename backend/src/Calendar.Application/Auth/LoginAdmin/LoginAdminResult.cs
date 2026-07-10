namespace Calendar.Application.Auth.LoginAdmin;

public sealed record LoginAdminResult(
    bool Succeeded,
    LoginAdminError Error,
    string? AccessToken,
    string? TokenType,
    DateTimeOffset? ExpiresAtUtc,
    Guid? AdminId,
    Guid? BusinessId,
    string? Email,
    string? DisplayName)
{
    public static LoginAdminResult Success(
        string accessToken,
        string tokenType,
        DateTimeOffset expiresAtUtc,
        Guid adminId,
        Guid businessId,
        string email,
        string displayName) => new(
            true,
            LoginAdminError.None,
            accessToken,
            tokenType,
            expiresAtUtc,
            adminId,
            businessId,
            email,
            displayName);

    public static LoginAdminResult Failure(LoginAdminError error) => new(
        false,
        error,
        null,
        null,
        null,
        null,
        null,
        null,
        null);
}
