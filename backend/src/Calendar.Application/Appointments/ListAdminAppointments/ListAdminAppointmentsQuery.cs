using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.ListAdminAppointments;

public sealed record ListAdminAppointmentsQuery(
    Guid BusinessId,
    DateOnly FromLocalDate,
    DateOnly ToLocalDate,
    Guid? StaffMemberId,
    Guid? ServiceId,
    AppointmentStatus? Status) : IQuery<ListAdminAppointmentsResult>;
