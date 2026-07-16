using Calendar.Application.Appointments.GetAppointmentDetails;

namespace Calendar.Application.Appointments.UpdateAppointmentStatus;

public sealed class UpdateAppointmentStatusResult
{
    private UpdateAppointmentStatusResult(bool succeeded, AppointmentDetails? appointment, UpdateAppointmentStatusError? error)
    {
        Succeeded = succeeded;
        Appointment = appointment;
        Error = error;
    }

    public bool Succeeded { get; }

    public AppointmentDetails? Appointment { get; }

    public UpdateAppointmentStatusError? Error { get; }

    public static UpdateAppointmentStatusResult Success(AppointmentDetails appointment) => new(true, appointment, null);

    public static UpdateAppointmentStatusResult Failure(UpdateAppointmentStatusError error) => new(false, null, error);
}
