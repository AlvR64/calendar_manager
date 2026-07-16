using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Appointments.GetAppointmentDetails;

public sealed record GetAppointmentDetailsQuery(Guid AppointmentId) : IQuery<AppointmentDetails?>;
