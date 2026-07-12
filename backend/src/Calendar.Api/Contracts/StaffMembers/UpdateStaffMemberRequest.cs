using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.StaffMembers;

/// <summary>Payload for updating a staff member.</summary>
public sealed record UpdateStaffMemberRequest
{
    /// <summary>The staff member display name.</summary>
    [Required]
    [MaxLength(150)]
    public required string DisplayName { get; init; }

    /// <summary>The staff member email address.</summary>
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; init; }

    /// <summary>The staff member phone number.</summary>
    [MaxLength(30)]
    public string? PhoneNumber { get; init; }

    /// <summary>The staff member biography.</summary>
    [MaxLength(1000)]
    public string? Bio { get; init; }

    /// <summary>The sort order used when displaying staff members.</summary>
    public int SortOrder { get; init; }
}
