namespace Calendar.Application.Appointments.Scheduling;

public sealed record AppointmentScheduleValidationRequest(
    Guid BusinessId,
    Guid ServiceId,
    Guid StaffMemberId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    Guid? ExcludedAppointmentId = null);
