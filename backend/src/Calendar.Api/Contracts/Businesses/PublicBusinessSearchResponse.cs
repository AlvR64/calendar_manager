namespace Calendar.Api.Contracts.Businesses;

/// <summary>Represents a paginated public business search result.</summary>
public sealed record PublicBusinessSearchResponse(
    IReadOnlyList<PublicBusinessCardResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage);

/// <summary>Represents a public business card for marketplace discovery.</summary>
public sealed record PublicBusinessCardResponse(
    Guid Id,
    string Slug,
    string Name,
    string? Description,
    string? City,
    string? CountryCode,
    string? Category,
    string TimeZoneId,
    string CurrencyCode,
    IReadOnlyList<PublicBusinessFeaturedServiceResponse> FeaturedServices,
    decimal? StartingPriceAmount);

/// <summary>Represents a lightweight public service shown in a business card.</summary>
public sealed record PublicBusinessFeaturedServiceResponse(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal PriceAmount);
