namespace Calendar.Api.Contracts.StaffMemberServices;

/// <summary>Payload for updating a staff member service assignment active state.</summary>
public sealed record UpdateStaffMemberServiceActiveStateRequest
{
    /// <summary>Whether the staff member service assignment is active.</summary>
    public bool IsActive { get; init; }
}
