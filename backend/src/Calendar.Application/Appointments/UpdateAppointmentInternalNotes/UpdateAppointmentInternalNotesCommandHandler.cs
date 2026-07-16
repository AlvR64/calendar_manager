using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Appointments.UpdateAppointmentInternalNotes;

public sealed class UpdateAppointmentInternalNotesCommandHandler(
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateAppointmentInternalNotesCommand, UpdateAppointmentInternalNotesResult>
{
    private const int MaxInternalNotesLength = 1000;

    public async Task<UpdateAppointmentInternalNotesResult> HandleAsync(UpdateAppointmentInternalNotesCommand command, CancellationToken cancellationToken)
    {
        var internalNotes = NormalizeOptionalText(command.InternalNotes);
        if (internalNotes?.Length > MaxInternalNotesLength)
        {
            return UpdateAppointmentInternalNotesResult.Failure(UpdateAppointmentInternalNotesError.InternalNotesTooLong);
        }

        var appointment = await appointmentRepository.GetByIdWithDetailsForUpdateAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return UpdateAppointmentInternalNotesResult.Failure(UpdateAppointmentInternalNotesError.AppointmentNotFound);
        }

        if (appointment.BusinessId != command.BusinessId)
        {
            return UpdateAppointmentInternalNotesResult.Failure(UpdateAppointmentInternalNotesError.Forbidden);
        }

        appointment.InternalNotes = internalNotes;
        appointment.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateAppointmentInternalNotesResult.Success(AppointmentDetailsMapper.Map(appointment, timeZoneProvider));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
