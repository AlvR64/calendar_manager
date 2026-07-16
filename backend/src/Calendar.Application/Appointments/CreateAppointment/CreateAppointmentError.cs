namespace Calendar.Application.Appointments.CreateAppointment;

public enum CreateAppointmentError
{
    None = 0,
    BusinessNotFound = 1,
    InvalidBusinessTimeZone = 2,
    ServiceNotFound = 3,
    StaffMemberNotFound = 4,
    StaffMemberServiceAssignmentNotFound = 5,
    InvalidTimeRange = 6,
    OutsideBookingWindow = 7,
    OutsideAvailability = 8,
    AppointmentOverlaps = 9,
    ConcurrentSlotConflict = 10
}
