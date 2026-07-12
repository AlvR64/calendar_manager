namespace Calendar.Application.StaffMemberAvailabilityExceptions.DeleteStaffMemberAvailabilityException;

public sealed record DeleteStaffMemberAvailabilityExceptionResult(
    bool Succeeded,
    DeleteStaffMemberAvailabilityExceptionError Error)
{
    public static DeleteStaffMemberAvailabilityExceptionResult Success() => new(
        true,
        DeleteStaffMemberAvailabilityExceptionError.None);

    public static DeleteStaffMemberAvailabilityExceptionResult Failure(DeleteStaffMemberAvailabilityExceptionError error) => new(
        false,
        error);
}
