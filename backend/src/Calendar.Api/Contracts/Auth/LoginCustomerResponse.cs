namespace Calendar.Api.Contracts.Auth;

/// <summary>Represents a successful customer authentication response.</summary>
public sealed record LoginCustomerResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    LoginCustomerUserResponse User);

/// <summary>Represents the authenticated customer account.</summary>
public sealed record LoginCustomerUserResponse(
    string Type,
    Guid Id,
    string Email,
    string FirstName,
    string? LastName);
