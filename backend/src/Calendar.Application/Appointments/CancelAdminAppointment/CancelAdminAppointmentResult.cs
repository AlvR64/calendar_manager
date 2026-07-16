using Calendar.Application.Appointments.GetAppointmentDetails;

namespace Calendar.Application.Appointments.CancelAdminAppointment;

public sealed class CancelAdminAppointmentResult
{
    private CancelAdminAppointmentResult(bool succeeded, AppointmentDetails? appointment, CancelAdminAppointmentError? error)
    {
        Succeeded = succeeded;
        Appointment = appointment;
        Error = error;
    }

    public bool Succeeded { get; }

    public AppointmentDetails? Appointment { get; }

    public CancelAdminAppointmentError? Error { get; }

    public static CancelAdminAppointmentResult Success(AppointmentDetails appointment) => new(true, appointment, null);

    public static CancelAdminAppointmentResult Failure(CancelAdminAppointmentError error) => new(false, null, error);
}
