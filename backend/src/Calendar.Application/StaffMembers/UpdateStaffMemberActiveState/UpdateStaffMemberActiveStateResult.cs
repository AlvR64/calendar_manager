using Calendar.Application.StaffMembers;

namespace Calendar.Application.StaffMembers.UpdateStaffMemberActiveState;

public sealed record UpdateStaffMemberActiveStateResult(
    bool Succeeded,
    UpdateStaffMemberActiveStateError Error,
    AdminStaffMemberDetails? StaffMember)
{
    public static UpdateStaffMemberActiveStateResult Success(AdminStaffMemberDetails staffMember) => new(
        true,
        UpdateStaffMemberActiveStateError.None,
        staffMember);

    public static UpdateStaffMemberActiveStateResult Failure(UpdateStaffMemberActiveStateError error) => new(
        false,
        error,
        null);
}
