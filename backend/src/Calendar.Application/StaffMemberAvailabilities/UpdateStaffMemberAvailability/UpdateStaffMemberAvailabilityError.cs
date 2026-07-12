namespace Calendar.Application.StaffMemberAvailabilities.UpdateStaffMemberAvailability;

public enum UpdateStaffMemberAvailabilityError
{
    None = 0,
    StaffMemberNotFound = 1,
    AvailabilityNotFound = 2,
    InvalidDayOfWeek = 3,
    InvalidTimeRange = 4,
    AvailabilityOverlaps = 5
}
