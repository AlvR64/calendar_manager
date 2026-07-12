namespace Calendar.Api.Contracts.Businesses;

/// <summary>Represents a public staff member to service assignment.</summary>
public sealed record BusinessStaffMemberServiceAssignmentResponse(
    Guid StaffMemberId,
    Guid ServiceId);
