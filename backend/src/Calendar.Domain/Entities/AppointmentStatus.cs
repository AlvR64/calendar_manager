namespace Calendar.Domain.Entities;

public enum AppointmentStatus
{
    Scheduled = 1,
    CancelledByCustomer = 2,
    CancelledByAdmin = 3,
    Completed = 4,
    NoShow = 5
}
