using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.UpdateAppointmentStatus;

public sealed class UpdateAppointmentStatusCommandHandler(
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateAppointmentStatusCommand, UpdateAppointmentStatusResult>
{
    public async Task<UpdateAppointmentStatusResult> HandleAsync(UpdateAppointmentStatusCommand command, CancellationToken cancellationToken)
    {
        if (command.Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Completed or AppointmentStatus.NoShow))
        {
            return UpdateAppointmentStatusResult.Failure(UpdateAppointmentStatusError.InvalidStatus);
        }

        var appointment = await appointmentRepository.GetByIdWithDetailsForUpdateAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return UpdateAppointmentStatusResult.Failure(UpdateAppointmentStatusError.AppointmentNotFound);
        }

        if (appointment.BusinessId != command.BusinessId)
        {
            return UpdateAppointmentStatusResult.Failure(UpdateAppointmentStatusError.Forbidden);
        }

        if (appointment.Status is AppointmentStatus.CancelledByCustomer or AppointmentStatus.CancelledByAdmin)
        {
            return UpdateAppointmentStatusResult.Failure(UpdateAppointmentStatusError.AppointmentCancelled);
        }

        appointment.Status = command.Status;
        appointment.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateAppointmentStatusResult.Success(AppointmentDetailsMapper.Map(appointment, timeZoneProvider));
    }
}
