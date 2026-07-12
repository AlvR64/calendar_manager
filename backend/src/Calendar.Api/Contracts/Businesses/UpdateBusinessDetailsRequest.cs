using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Businesses;

/// <summary>Payload for updating the current admin business public details.</summary>
public sealed record UpdateBusinessDetailsRequest
{
    /// <summary>The public business name.</summary>
    [Required]
    [MaxLength(150)]
    public required string Name { get; init; }

    /// <summary>The public business description.</summary>
    [MaxLength(1000)]
    public string? Description { get; init; }

    /// <summary>The public contact email address.</summary>
    [EmailAddress]
    [MaxLength(255)]
    public string? ContactEmail { get; init; }

    /// <summary>The public contact phone number.</summary>
    [MaxLength(30)]
    public string? ContactPhoneNumber { get; init; }

    /// <summary>The public website URL.</summary>
    [Url]
    [MaxLength(500)]
    public string? WebsiteUrl { get; init; }

    /// <summary>The first public address line.</summary>
    [MaxLength(200)]
    public string? AddressLine1 { get; init; }

    /// <summary>The second public address line.</summary>
    [MaxLength(200)]
    public string? AddressLine2 { get; init; }

    /// <summary>The public business city.</summary>
    [MaxLength(100)]
    public string? City { get; init; }

    /// <summary>The public postal code.</summary>
    [MaxLength(20)]
    public string? PostalCode { get; init; }

    /// <summary>The ISO 3166-1 alpha-2 country code.</summary>
    [StringLength(2, MinimumLength = 2)]
    [RegularExpression("^[A-Za-z]{2}$")]
    public string? CountryCode { get; init; }

    /// <summary>The business time zone identifier.</summary>
    [Required]
    [MaxLength(100)]
    public required string TimeZoneId { get; init; }

    /// <summary>The ISO 4217 currency code used by the business.</summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    [RegularExpression("^[A-Za-z]{3}$")]
    public required string CurrencyCode { get; init; }
}
