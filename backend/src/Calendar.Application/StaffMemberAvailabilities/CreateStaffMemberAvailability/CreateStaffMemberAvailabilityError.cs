namespace Calendar.Application.StaffMemberAvailabilities.CreateStaffMemberAvailability;

public enum CreateStaffMemberAvailabilityError
{
    None = 0,
    StaffMemberNotFound = 1,
    InvalidDayOfWeek = 2,
    InvalidTimeRange = 3,
    AvailabilityOverlaps = 4
}
