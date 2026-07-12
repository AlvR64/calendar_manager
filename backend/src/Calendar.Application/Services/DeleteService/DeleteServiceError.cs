namespace Calendar.Application.Services.DeleteService;

public enum DeleteServiceError
{
    None = 0,
    ServiceNotFound = 1,
    ServiceHasAppointments = 2
}
