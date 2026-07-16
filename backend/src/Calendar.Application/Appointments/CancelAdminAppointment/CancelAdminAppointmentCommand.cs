using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;

namespace Calendar.Application.Appointments.CancelAdminAppointment;

public sealed record CancelAdminAppointmentCommand(
    Guid BusinessId,
    Guid AppointmentId,
    string? CancellationReason) : ICommand<CancelAdminAppointmentResult>;
