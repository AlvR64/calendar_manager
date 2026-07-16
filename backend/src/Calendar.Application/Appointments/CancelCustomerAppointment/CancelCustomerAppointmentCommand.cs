using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Appointments.CancelCustomerAppointment;

public sealed record CancelCustomerAppointmentCommand(
    Guid CustomerId,
    Guid AppointmentId,
    string? CancellationReason) : ICommand<CancelCustomerAppointmentResult>;
