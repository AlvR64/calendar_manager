namespace Calendar.Api.Contracts.Businesses;

/// <summary>Represents a public service offered by a business.</summary>
public sealed record BusinessServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal PriceAmount,
    int SortOrder);
