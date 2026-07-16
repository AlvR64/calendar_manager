using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Appointments.UpdateAppointmentInternalNotes;

public sealed record UpdateAppointmentInternalNotesCommand(
    Guid BusinessId,
    Guid AppointmentId,
    string? InternalNotes) : ICommand<UpdateAppointmentInternalNotesResult>;
