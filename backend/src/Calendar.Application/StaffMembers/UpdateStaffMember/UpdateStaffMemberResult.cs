using Calendar.Application.StaffMembers;

namespace Calendar.Application.StaffMembers.UpdateStaffMember;

public sealed record UpdateStaffMemberResult(
    bool Succeeded,
    UpdateStaffMemberError Error,
    AdminStaffMemberDetails? StaffMember)
{
    public static UpdateStaffMemberResult Success(AdminStaffMemberDetails staffMember) => new(
        true,
        UpdateStaffMemberError.None,
        staffMember);

    public static UpdateStaffMemberResult Failure(UpdateStaffMemberError error) => new(
        false,
        error,
        null);
}
