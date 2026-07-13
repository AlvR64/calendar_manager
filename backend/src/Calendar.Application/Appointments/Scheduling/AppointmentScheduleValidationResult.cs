namespace Calendar.Application.Appointments.Scheduling;

public sealed record AppointmentScheduleValidationResult(
    bool Succeeded,
    AppointmentScheduleValidationError Error)
{
    public static AppointmentScheduleValidationResult Success() => new(true, AppointmentScheduleValidationError.None);

    public static AppointmentScheduleValidationResult Failure(AppointmentScheduleValidationError error) => new(false, error);
}
