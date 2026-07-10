namespace Calendar.Domain.Abstractions;

public sealed record AccessToken(
    string Token,
    string TokenType,
    DateTimeOffset ExpiresAtUtc);
