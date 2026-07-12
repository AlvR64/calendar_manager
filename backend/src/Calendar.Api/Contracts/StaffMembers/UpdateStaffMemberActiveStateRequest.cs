namespace Calendar.Api.Contracts.StaffMembers;

/// <summary>Payload for updating a staff member active state.</summary>
public sealed record UpdateStaffMemberActiveStateRequest
{
    /// <summary>Whether the staff member is active.</summary>
    public bool IsActive { get; init; }
}
