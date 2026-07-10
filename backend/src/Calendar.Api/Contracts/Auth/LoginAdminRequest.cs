using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Auth;

/// <summary>Payload for authenticating an admin account.</summary>
public sealed record LoginAdminRequest
{
    /// <summary>The admin email address.</summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; init; }

    /// <summary>The admin password.</summary>
    [Required]
    [MaxLength(200)]
    public required string Password { get; init; }
}
