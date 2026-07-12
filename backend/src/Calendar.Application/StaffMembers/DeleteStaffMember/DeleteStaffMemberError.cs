namespace Calendar.Application.StaffMembers.DeleteStaffMember;

public enum DeleteStaffMemberError
{
    None = 0,
    StaffMemberNotFound = 1,
    StaffMemberHasAppointments = 2
}
