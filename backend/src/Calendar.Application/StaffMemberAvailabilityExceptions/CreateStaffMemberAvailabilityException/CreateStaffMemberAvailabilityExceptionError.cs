namespace Calendar.Application.StaffMemberAvailabilityExceptions.CreateStaffMemberAvailabilityException;

public enum CreateStaffMemberAvailabilityExceptionError
{
    None = 0,
    StaffMemberNotFound = 1,
    InvalidClosedException = 2,
    InvalidTimeRange = 3,
    AvailabilityExceptionAlreadyExists = 4,
    AvailabilityExceptionOverlaps = 5,
    ReasonTooLong = 6
}
