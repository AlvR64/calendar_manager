namespace Calendar.Application.StaffMemberServices.AssignStaffMemberService;

public sealed record AssignStaffMemberServiceResult(
    bool Succeeded,
    AssignStaffMemberServiceError Error,
    Guid? StaffMemberId,
    Guid? ServiceId,
    bool? IsActive,
    DateTimeOffset? CreatedAtUtc)
{
    public static AssignStaffMemberServiceResult Success(
        Guid staffMemberId,
        Guid serviceId,
        bool isActive,
        DateTimeOffset createdAtUtc) => new(
            true,
            AssignStaffMemberServiceError.None,
            staffMemberId,
            serviceId,
            isActive,
            createdAtUtc);

    public static AssignStaffMemberServiceResult Failure(AssignStaffMemberServiceError error) => new(
        false,
        error,
        null,
        null,
        null,
        null);
}
