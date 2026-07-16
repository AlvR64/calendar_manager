using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.CancelCustomerAppointment;

public sealed class CancelCustomerAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelCustomerAppointmentCommand, CancelCustomerAppointmentResult>
{
    public async Task<CancelCustomerAppointmentResult> HandleAsync(CancelCustomerAppointmentCommand command, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdWithDetailsForUpdateAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return CancelCustomerAppointmentResult.Failure(CancelCustomerAppointmentError.AppointmentNotFound);
        }

        if (appointment.CustomerId != command.CustomerId)
        {
            return CancelCustomerAppointmentResult.Failure(CancelCustomerAppointmentError.Forbidden);
        }

        if (appointment.Status is AppointmentStatus.CancelledByCustomer or AppointmentStatus.CancelledByAdmin)
        {
            return CancelCustomerAppointmentResult.Failure(CancelCustomerAppointmentError.AlreadyCancelled);
        }

        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            return CancelCustomerAppointmentResult.Failure(CancelCustomerAppointmentError.AppointmentNotCancelable);
        }

        var now = DateTimeOffset.UtcNow;
        if (appointment.StartAtUtc <= now)
        {
            return CancelCustomerAppointmentResult.Failure(CancelCustomerAppointmentError.AppointmentInPast);
        }

        appointment.Status = AppointmentStatus.CancelledByCustomer;
        appointment.CancelledAtUtc = now;
        appointment.CancellationReason = NormalizeOptionalText(command.CancellationReason);
        appointment.UpdatedAtUtc = now;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CancelCustomerAppointmentResult.Success(AppointmentDetailsMapper.Map(appointment, timeZoneProvider));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
