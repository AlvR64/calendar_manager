namespace Calendar.Application.Appointments.CreateAppointment;

public interface IAppointmentCreationConcurrencyGuard
{
    Task<T> ExecuteWithLockAsync<T>(
        string resource,
        Func<CancellationToken, Task<T>> action,
        Func<T> lockNotAcquiredResult,
        CancellationToken cancellationToken);
}
