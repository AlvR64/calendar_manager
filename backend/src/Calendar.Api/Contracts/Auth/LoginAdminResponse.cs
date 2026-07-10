namespace Calendar.Api.Contracts.Auth;

/// <summary>Represents a successful admin authentication response.</summary>
public sealed record LoginAdminResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    LoginAdminUserResponse User);

/// <summary>Represents the authenticated admin account.</summary>
public sealed record LoginAdminUserResponse(
    string Type,
    Guid Id,
    Guid BusinessId,
    string Email,
    string DisplayName);
