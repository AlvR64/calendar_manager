using Calendar.Application.StaffMemberAvailabilityExceptions;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.UpdateStaffMemberAvailabilityException;

public sealed record UpdateStaffMemberAvailabilityExceptionResult(
    bool Succeeded,
    UpdateStaffMemberAvailabilityExceptionError Error,
    StaffMemberAvailabilityExceptionDetails? Exception)
{
    public static UpdateStaffMemberAvailabilityExceptionResult Success(StaffMemberAvailabilityExceptionDetails exception) => new(
        true,
        UpdateStaffMemberAvailabilityExceptionError.None,
        exception);

    public static UpdateStaffMemberAvailabilityExceptionResult Failure(UpdateStaffMemberAvailabilityExceptionError error) => new(
        false,
        error,
        null);
}
