using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Services;

/// <summary>Payload for updating a service.</summary>
public sealed record UpdateServiceRequest
{
    /// <summary>The service name.</summary>
    [Required]
    [MaxLength(150)]
    public required string Name { get; init; }

    /// <summary>The service description.</summary>
    [MaxLength(1000)]
    public string? Description { get; init; }

    /// <summary>The service duration in minutes.</summary>
    [Range(1, 1440)]
    public int DurationMinutes { get; init; }

    /// <summary>The service price amount.</summary>
    [Range(0, 999999.99)]
    public decimal PriceAmount { get; init; }

    /// <summary>The sort order used when displaying services.</summary>
    public int SortOrder { get; init; }
}
