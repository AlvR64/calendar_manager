using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Auth.RegisterBusiness;

public sealed record RegisterBusinessCommand(
    string BusinessName,
    string BusinessSlug,
    string TimeZoneId,
    string CurrencyCode,
    string AdminEmail,
    string AdminPassword,
    string AdminDisplayName) : ICommand<RegisterBusinessResult>;
