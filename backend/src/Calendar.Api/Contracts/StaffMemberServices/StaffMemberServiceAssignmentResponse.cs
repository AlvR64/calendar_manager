namespace Calendar.Api.Contracts.StaffMemberServices;

/// <summary>Represents a staff member assignment to a service.</summary>
public sealed record StaffMemberServiceAssignmentResponse(
    Guid StaffMemberId,
    Guid ServiceId,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
