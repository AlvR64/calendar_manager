namespace Calendar.Api.Contracts.Businesses;

/// <summary>Represents the public profile payload for a business page.</summary>
public sealed record BusinessProfileResponse(
    BusinessResponse Business,
    IReadOnlyList<BusinessServiceResponse> Services,
    IReadOnlyList<BusinessStaffMemberResponse> StaffMembers,
    IReadOnlyList<BusinessStaffMemberServiceAssignmentResponse> Assignments);
