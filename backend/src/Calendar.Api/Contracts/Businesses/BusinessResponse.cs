namespace Calendar.Api.Contracts.Businesses;

/// <summary>Represents the public business information.</summary>
public sealed record BusinessResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ContactEmail,
    string? ContactPhoneNumber,
    string? WebsiteUrl,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? PostalCode,
    string? CountryCode,
    string TimeZoneId,
    string CurrencyCode);
