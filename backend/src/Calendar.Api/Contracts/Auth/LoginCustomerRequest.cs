using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Auth;

/// <summary>Payload for authenticating a customer account.</summary>
public sealed record LoginCustomerRequest
{
    /// <summary>The customer email address.</summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; init; }

    /// <summary>The customer password.</summary>
    [Required]
    [MaxLength(200)]
    public required string Password { get; init; }
}
