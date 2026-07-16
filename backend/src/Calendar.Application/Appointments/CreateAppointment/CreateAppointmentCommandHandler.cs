using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.Scheduling;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.CreateAppointment;

public sealed class CreateAppointmentCommandHandler(
    IBusinessRepository businessRepository,
    IServiceRepository serviceRepository,
    IAppointmentRepository appointmentRepository,
    IAppointmentScheduleValidator appointmentScheduleValidator,
    IAppointmentCreationConcurrencyGuard concurrencyGuard,
    ITimeZoneProvider timeZoneProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateAppointmentCommand, CreateAppointmentResult>
{
    public async Task<CreateAppointmentResult> HandleAsync(
        CreateAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var startAtUtc = command.StartAtUtc.ToUniversalTime();
        var business = await businessRepository.GetActiveByIdAsync(command.BusinessId, cancellationToken);
        if (business is null)
        {
            return CreateAppointmentResult.Failure(CreateAppointmentError.BusinessNotFound);
        }

        if (!timeZoneProvider.TryGetIanaTimeZoneInfo(business.TimeZoneId, out var businessTimeZone))
        {
            return CreateAppointmentResult.Failure(CreateAppointmentError.InvalidBusinessTimeZone);
        }

        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(startAtUtc, businessTimeZone).DateTime);
        var lockResource = $"appointment:{command.BusinessId}:{command.StaffMemberId}:{localDate:yyyy-MM-dd}";

        return await concurrencyGuard.ExecuteWithLockAsync(
            lockResource,
            ct => CreateAppointmentInsideLockAsync(command, startAtUtc, business.CurrencyCode, ct),
            () => CreateAppointmentResult.Failure(CreateAppointmentError.ConcurrentSlotConflict),
            cancellationToken);
    }

    private async Task<CreateAppointmentResult> CreateAppointmentInsideLockAsync(
        CreateAppointmentCommand command,
        DateTimeOffset startAtUtc,
        string currencyCode,
        CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetActiveByIdAndBusinessIdAsync(
            command.ServiceId,
            command.BusinessId,
            cancellationToken);

        if (service is null)
        {
            return CreateAppointmentResult.Failure(CreateAppointmentError.ServiceNotFound);
        }

        var endAtUtc = startAtUtc.AddMinutes(service.DurationMinutes);
        var validationResult = await appointmentScheduleValidator.ValidateAsync(
            new AppointmentScheduleValidationRequest(
                command.BusinessId,
                command.ServiceId,
                command.StaffMemberId,
                startAtUtc,
                endAtUtc),
            cancellationToken);

        if (!validationResult.Succeeded)
        {
            return CreateAppointmentResult.Failure(MapScheduleValidationError(validationResult.Error));
        }

        var now = DateTimeOffset.UtcNow;
        var customerNotes = NormalizeOptionalText(command.CustomerNotes);
        var appointmentId = Guid.NewGuid();
        var appointment = new Appointment
        {
            Id = appointmentId,
            BusinessId = command.BusinessId,
            StaffMemberId = command.StaffMemberId,
            ServiceId = command.ServiceId,
            CustomerId = command.CustomerId,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Status = AppointmentStatus.Scheduled,
            CustomerNotes = customerNotes,
            ServiceNameSnapshot = service.Name,
            ServiceDurationMinutesSnapshot = service.DurationMinutes,
            PriceAmountSnapshot = service.PriceAmount,
            CurrencyCodeSnapshot = currencyCode,
            CreatedAtUtc = now
        };

        appointmentRepository.Add(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAppointmentResult.Success(
            appointmentId,
            command.BusinessId,
            command.StaffMemberId,
            command.ServiceId,
            command.CustomerId,
            startAtUtc,
            endAtUtc,
            appointment.Status.ToString(),
            customerNotes,
            service.Name,
            service.DurationMinutes,
            service.PriceAmount,
            currencyCode,
            now);
    }

    private static CreateAppointmentError MapScheduleValidationError(AppointmentScheduleValidationError error) => error switch
    {
        AppointmentScheduleValidationError.BusinessNotFound => CreateAppointmentError.BusinessNotFound,
        AppointmentScheduleValidationError.InvalidBusinessTimeZone => CreateAppointmentError.InvalidBusinessTimeZone,
        AppointmentScheduleValidationError.ServiceNotFound => CreateAppointmentError.ServiceNotFound,
        AppointmentScheduleValidationError.StaffMemberNotFound => CreateAppointmentError.StaffMemberNotFound,
        AppointmentScheduleValidationError.StaffMemberServiceAssignmentNotFound => CreateAppointmentError.StaffMemberServiceAssignmentNotFound,
        AppointmentScheduleValidationError.InvalidTimeRange => CreateAppointmentError.InvalidTimeRange,
        AppointmentScheduleValidationError.OutsideBookingWindow => CreateAppointmentError.OutsideBookingWindow,
        AppointmentScheduleValidationError.OutsideAvailability => CreateAppointmentError.OutsideAvailability,
        AppointmentScheduleValidationError.AppointmentOverlaps => CreateAppointmentError.AppointmentOverlaps,
        _ => CreateAppointmentError.InvalidTimeRange
    };

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
