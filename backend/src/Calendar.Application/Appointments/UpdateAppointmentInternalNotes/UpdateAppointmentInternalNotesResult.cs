using Calendar.Application.Appointments.GetAppointmentDetails;

namespace Calendar.Application.Appointments.UpdateAppointmentInternalNotes;

public sealed class UpdateAppointmentInternalNotesResult
{
    private UpdateAppointmentInternalNotesResult(bool succeeded, AppointmentDetails? appointment, UpdateAppointmentInternalNotesError? error)
    {
        Succeeded = succeeded;
        Appointment = appointment;
        Error = error;
    }

    public bool Succeeded { get; }

    public AppointmentDetails? Appointment { get; }

    public UpdateAppointmentInternalNotesError? Error { get; }

    public static UpdateAppointmentInternalNotesResult Success(AppointmentDetails appointment) => new(true, appointment, null);

    public static UpdateAppointmentInternalNotesResult Failure(UpdateAppointmentInternalNotesError error) => new(false, null, error);
}
