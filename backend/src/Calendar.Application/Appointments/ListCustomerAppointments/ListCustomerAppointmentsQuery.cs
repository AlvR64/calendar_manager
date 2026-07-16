using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.ListCustomerAppointments;

public sealed record ListCustomerAppointmentsQuery(
    Guid CustomerId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    AppointmentStatus? Status) : IQuery<IReadOnlyList<AppointmentDetails>>;
