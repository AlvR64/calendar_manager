namespace Calendar.Api.Contracts.Auth;

/// <summary>Represents a newly registered customer account.</summary>
public sealed record RegisterCustomerResponse(
    Guid CustomerId,
    string Email,
    string FirstName,
    string? LastName,
    string? PhoneNumber,
    DateTimeOffset CreatedAtUtc);
