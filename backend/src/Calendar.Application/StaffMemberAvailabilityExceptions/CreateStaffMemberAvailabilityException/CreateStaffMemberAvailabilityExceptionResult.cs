using Calendar.Application.StaffMemberAvailabilityExceptions;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.CreateStaffMemberAvailabilityException;

public sealed record CreateStaffMemberAvailabilityExceptionResult(
    bool Succeeded,
    CreateStaffMemberAvailabilityExceptionError Error,
    StaffMemberAvailabilityExceptionDetails? Exception)
{
    public static CreateStaffMemberAvailabilityExceptionResult Success(StaffMemberAvailabilityExceptionDetails exception) => new(
        true,
        CreateStaffMemberAvailabilityExceptionError.None,
        exception);

    public static CreateStaffMemberAvailabilityExceptionResult Failure(CreateStaffMemberAvailabilityExceptionError error) => new(
        false,
        error,
        null);
}
