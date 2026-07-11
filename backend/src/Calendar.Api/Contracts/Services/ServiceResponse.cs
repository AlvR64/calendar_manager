namespace Calendar.Api.Contracts.Services;

/// <summary>Represents a service offered by a business.</summary>
public sealed record ServiceResponse(
    Guid Id,
    Guid BusinessId,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal PriceAmount,
    bool IsActive,
    int SortOrder,
    DateTimeOffset CreatedAtUtc);
