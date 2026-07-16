namespace Calendar.Application.Appointments.CancelCustomerAppointment;

public enum CancelCustomerAppointmentError
{
    None = 0,
    AppointmentNotFound = 1,
    Forbidden = 2,
    AlreadyCancelled = 3,
    AppointmentNotCancelable = 4,
    AppointmentInPast = 5
}
