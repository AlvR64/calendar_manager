using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Auth;

/// <summary>Payload for registering a customer account.</summary>
public sealed record RegisterCustomerRequest
{
    /// <summary>The customer email address.</summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; init; }

    /// <summary>The customer password.</summary>
    [Required]
    [MinLength(8)]
    [MaxLength(200)]
    public required string Password { get; init; }

    /// <summary>The customer first name.</summary>
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; init; }

    /// <summary>The customer last name.</summary>
    [MaxLength(100)]
    public string? LastName { get; init; }

    /// <summary>The customer phone number.</summary>
    [MaxLength(30)]
    public string? PhoneNumber { get; init; }
}
