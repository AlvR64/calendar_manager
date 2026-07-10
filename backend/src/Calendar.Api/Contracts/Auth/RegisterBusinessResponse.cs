namespace Calendar.Api.Contracts.Auth;

/// <summary>Represents a newly registered business and its initial admin account.</summary>
public sealed record RegisterBusinessResponse(
    Guid BusinessId,
    string BusinessSlug,
    Guid AdminId,
    string AdminEmail,
    DateTimeOffset CreatedAtUtc);
