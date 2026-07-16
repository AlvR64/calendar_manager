using Calendar.Application.Appointments.GetAppointmentDetails;

namespace Calendar.Application.Appointments.ListAdminAppointments;

public sealed class ListAdminAppointmentsResult
{
    private ListAdminAppointmentsResult(bool succeeded, IReadOnlyList<AppointmentDetails> appointments, ListAdminAppointmentsError? error)
    {
        Succeeded = succeeded;
        Appointments = appointments;
        Error = error;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<AppointmentDetails> Appointments { get; }

    public ListAdminAppointmentsError? Error { get; }

    public static ListAdminAppointmentsResult Success(IReadOnlyList<AppointmentDetails> appointments) => new(true, appointments, null);

    public static ListAdminAppointmentsResult Failure(ListAdminAppointmentsError error) => new(false, [], error);
}
