namespace Calendar.Application.StaffMemberAvailabilities.DeleteStaffMemberAvailability;

public sealed record DeleteStaffMemberAvailabilityResult(bool Succeeded, DeleteStaffMemberAvailabilityError Error)
{
    public static DeleteStaffMemberAvailabilityResult Success() => new(true, DeleteStaffMemberAvailabilityError.None);

    public static DeleteStaffMemberAvailabilityResult Failure(DeleteStaffMemberAvailabilityError error) => new(false, error);
}
