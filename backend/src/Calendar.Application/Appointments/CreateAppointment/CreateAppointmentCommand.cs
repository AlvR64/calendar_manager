using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Appointments.CreateAppointment;

public sealed record CreateAppointmentCommand(
    Guid CustomerId,
    Guid BusinessId,
    Guid ServiceId,
    Guid StaffMemberId,
    DateTimeOffset StartAtUtc,
    string? CustomerNotes) : ICommand<CreateAppointmentResult>;
