using Calendar.Application.StaffMemberAvailabilities;

namespace Calendar.Application.StaffMemberAvailabilities.CreateStaffMemberAvailability;

public sealed record CreateStaffMemberAvailabilityResult(
    bool Succeeded,
    CreateStaffMemberAvailabilityError Error,
    StaffMemberAvailabilityDetails? Availability)
{
    public static CreateStaffMemberAvailabilityResult Success(StaffMemberAvailabilityDetails availability) => new(
        true,
        CreateStaffMemberAvailabilityError.None,
        availability);

    public static CreateStaffMemberAvailabilityResult Failure(CreateStaffMemberAvailabilityError error) => new(
        false,
        error,
        null);
}
