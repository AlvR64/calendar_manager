using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Businesses.UpdateBusinessDetails;

public sealed record UpdateBusinessDetailsCommand(
    Guid BusinessId,
    string Name,
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
    string CurrencyCode) : ICommand<UpdateBusinessDetailsResult>;
