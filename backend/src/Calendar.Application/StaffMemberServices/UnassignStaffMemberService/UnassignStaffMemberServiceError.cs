namespace Calendar.Application.StaffMemberServices.UnassignStaffMemberService;

public enum UnassignStaffMemberServiceError
{
    None = 0,
    StaffMemberNotFound = 1,
    ServiceNotFound = 2,
    AssignmentNotFound = 3,
    AssignmentHasAppointments = 4
}
