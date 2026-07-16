using Calendar.Application.Appointments.GetAppointmentDetails;

namespace Calendar.Application.Appointments.CancelCustomerAppointment;

public sealed record CancelCustomerAppointmentResult
{
    private CancelCustomerAppointmentResult(AppointmentDetails? appointment, CancelCustomerAppointmentError error)
    {
        Appointment = appointment;
        Error = error;
    }

    public bool Succeeded => Error == CancelCustomerAppointmentError.None;

    public AppointmentDetails? Appointment { get; }

    public CancelCustomerAppointmentError Error { get; }

    public static CancelCustomerAppointmentResult Success(AppointmentDetails appointment) => new(appointment, CancelCustomerAppointmentError.None);

    public static CancelCustomerAppointmentResult Failure(CancelCustomerAppointmentError error) => new(null, error);
}
