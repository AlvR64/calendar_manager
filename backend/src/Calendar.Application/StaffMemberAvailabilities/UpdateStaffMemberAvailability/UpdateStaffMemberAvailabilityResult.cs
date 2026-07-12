using Calendar.Application.StaffMemberAvailabilities;

namespace Calendar.Application.StaffMemberAvailabilities.UpdateStaffMemberAvailability;

public sealed record UpdateStaffMemberAvailabilityResult(
    bool Succeeded,
    UpdateStaffMemberAvailabilityError Error,
    StaffMemberAvailabilityDetails? Availability)
{
    public static UpdateStaffMemberAvailabilityResult Success(StaffMemberAvailabilityDetails availability) => new(
        true,
        UpdateStaffMemberAvailabilityError.None,
        availability);

    public static UpdateStaffMemberAvailabilityResult Failure(UpdateStaffMemberAvailabilityError error) => new(
        false,
        error,
        null);
}
