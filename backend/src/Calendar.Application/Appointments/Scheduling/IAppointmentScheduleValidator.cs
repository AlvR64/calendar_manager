namespace Calendar.Application.Appointments.Scheduling;

public interface IAppointmentScheduleValidator
{
    Task<AppointmentScheduleValidationResult> ValidateAsync(
        AppointmentScheduleValidationRequest request,
        CancellationToken cancellationToken);
}
