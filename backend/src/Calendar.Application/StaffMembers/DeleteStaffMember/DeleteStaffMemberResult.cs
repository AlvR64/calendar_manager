namespace Calendar.Application.StaffMembers.DeleteStaffMember;

public sealed record DeleteStaffMemberResult(bool Succeeded, DeleteStaffMemberError Error)
{
    public static DeleteStaffMemberResult Success() => new(true, DeleteStaffMemberError.None);

    public static DeleteStaffMemberResult Failure(DeleteStaffMemberError error) => new(false, error);
}
