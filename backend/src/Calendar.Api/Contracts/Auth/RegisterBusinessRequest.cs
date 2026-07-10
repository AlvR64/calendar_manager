using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Auth;

/// <summary>Payload for registering a business with its initial admin account.</summary>
public sealed record RegisterBusinessRequest
{
    /// <summary>The public business name.</summary>
    [Required]
    [MaxLength(150)]
    public required string BusinessName { get; init; }

    /// <summary>The unique URL-friendly business slug.</summary>
    [Required]
    [MaxLength(120)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public required string BusinessSlug { get; init; }

    /// <summary>The business time zone identifier.</summary>
    [Required]
    [MaxLength(100)]
    public required string TimeZoneId { get; init; }

    /// <summary>The ISO 4217 currency code used by the business.</summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    [RegularExpression("^[A-Za-z]{3}$")]
    public required string CurrencyCode { get; init; }

    /// <summary>The admin email address.</summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string AdminEmail { get; init; }

    /// <summary>The admin password.</summary>
    [Required]
    [MinLength(8)]
    [MaxLength(200)]
    public required string AdminPassword { get; init; }

    /// <summary>The admin display name.</summary>
    [Required]
    [MaxLength(150)]
    public required string AdminDisplayName { get; init; }
}
