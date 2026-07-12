namespace Calendar.Application.StaffMemberServices.UnassignStaffMemberService;

public sealed record UnassignStaffMemberServiceResult(bool Succeeded, UnassignStaffMemberServiceError Error)
{
    public static UnassignStaffMemberServiceResult Success() => new(true, UnassignStaffMemberServiceError.None);

    public static UnassignStaffMemberServiceResult Failure(UnassignStaffMemberServiceError error) => new(false, error);
}
