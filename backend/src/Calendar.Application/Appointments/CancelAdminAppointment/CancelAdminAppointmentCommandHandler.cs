using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.CancelAdminAppointment;

public sealed class CancelAdminAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelAdminAppointmentCommand, CancelAdminAppointmentResult>
{
    public async Task<CancelAdminAppointmentResult> HandleAsync(CancelAdminAppointmentCommand command, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdWithDetailsForUpdateAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return CancelAdminAppointmentResult.Failure(CancelAdminAppointmentError.AppointmentNotFound);
        }

        if (appointment.BusinessId != command.BusinessId)
        {
            return CancelAdminAppointmentResult.Failure(CancelAdminAppointmentError.Forbidden);
        }

        if (appointment.Status is AppointmentStatus.CancelledByCustomer or AppointmentStatus.CancelledByAdmin)
        {
            return CancelAdminAppointmentResult.Failure(CancelAdminAppointmentError.AlreadyCancelled);
        }

        var now = DateTimeOffset.UtcNow;
        appointment.Status = AppointmentStatus.CancelledByAdmin;
        appointment.CancelledAtUtc = now;
        appointment.CancellationReason = NormalizeOptionalText(command.CancellationReason);
        appointment.UpdatedAtUtc = now;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CancelAdminAppointmentResult.Success(AppointmentDetailsMapper.Map(appointment, timeZoneProvider));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
