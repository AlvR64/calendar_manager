using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.UpdateAppointmentStatus;

public sealed record UpdateAppointmentStatusCommand(
    Guid BusinessId,
    Guid AppointmentId,
    AppointmentStatus Status) : ICommand<UpdateAppointmentStatusResult>;
