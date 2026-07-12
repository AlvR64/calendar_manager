namespace Calendar.Application.StaffMemberAvailabilityExceptions.UpdateStaffMemberAvailabilityException;

public enum UpdateStaffMemberAvailabilityExceptionError
{
    None = 0,
    StaffMemberNotFound = 1,
    AvailabilityExceptionNotFound = 2,
    InvalidClosedException = 3,
    InvalidTimeRange = 4,
    AvailabilityExceptionAlreadyExists = 5,
    AvailabilityExceptionOverlaps = 6,
    ReasonTooLong = 7
}
