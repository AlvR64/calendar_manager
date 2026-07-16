using Calendar.Application.Appointments.CancelAdminAppointment;
using Calendar.Application.Appointments.UpdateAppointmentInternalNotes;
using Calendar.Application.Appointments.UpdateAppointmentStatus;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Appointments.AdminAppointmentActions;

public sealed class AdminAppointmentActionsCommandHandlerTests
{
    [Fact]
    public async Task CancelAdminAppointment_WhenAppointmentBelongsToBusiness_CancelsAppointment()
    {
        var businessId = Guid.NewGuid();
        var appointment = CreateAppointment(businessId, AppointmentStatus.Scheduled);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelAdminAppointmentCommandHandler(new FakeAppointmentRepository(appointment), new FakeTimeZoneProvider(), unitOfWork);

        var result = await handler.HandleAsync(new CancelAdminAppointmentCommand(businessId, appointment.Id, " Closed early "), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.CancelledByAdmin);
        appointment.CancellationReason.Should().Be("Closed early");
        appointment.CancelledAtUtc.Should().NotBeNull();
        appointment.UpdatedAtUtc.Should().NotBeNull();
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task CancelAdminAppointment_WhenBusinessDoesNotMatch_ReturnsForbidden()
    {
        var appointment = CreateAppointment(Guid.NewGuid(), AppointmentStatus.Scheduled);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelAdminAppointmentCommandHandler(new FakeAppointmentRepository(appointment), new FakeTimeZoneProvider(), unitOfWork);

        var result = await handler.HandleAsync(new CancelAdminAppointmentCommand(Guid.NewGuid(), appointment.Id, null), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(CancelAdminAppointmentError.Forbidden);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_WhenRequestedStatusIsCancelled_ReturnsInvalidStatus()
    {
        var appointment = CreateAppointment(Guid.NewGuid(), AppointmentStatus.Scheduled);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateAppointmentStatusCommandHandler(new FakeAppointmentRepository(appointment), new FakeTimeZoneProvider(), unitOfWork);

        var result = await handler.HandleAsync(new UpdateAppointmentStatusCommand(appointment.BusinessId, appointment.Id, AppointmentStatus.CancelledByAdmin), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(UpdateAppointmentStatusError.InvalidStatus);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_WhenAppointmentIsCancelled_ReturnsAppointmentCancelled()
    {
        var appointment = CreateAppointment(Guid.NewGuid(), AppointmentStatus.CancelledByCustomer);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateAppointmentStatusCommandHandler(new FakeAppointmentRepository(appointment), new FakeTimeZoneProvider(), unitOfWork);

        var result = await handler.HandleAsync(new UpdateAppointmentStatusCommand(appointment.BusinessId, appointment.Id, AppointmentStatus.Completed), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(UpdateAppointmentStatusError.AppointmentCancelled);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task UpdateAppointmentInternalNotes_WhenValid_TrimsAndSavesNotes()
    {
        var appointment = CreateAppointment(Guid.NewGuid(), AppointmentStatus.Scheduled);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateAppointmentInternalNotesCommandHandler(new FakeAppointmentRepository(appointment), new FakeTimeZoneProvider(), unitOfWork);

        var result = await handler.HandleAsync(new UpdateAppointmentInternalNotesCommand(appointment.BusinessId, appointment.Id, " Prep room 2 "), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        appointment.InternalNotes.Should().Be("Prep room 2");
        appointment.UpdatedAtUtc.Should().NotBeNull();
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAppointmentInternalNotes_WhenTooLong_ReturnsInternalNotesTooLong()
    {
        var appointment = CreateAppointment(Guid.NewGuid(), AppointmentStatus.Scheduled);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateAppointmentInternalNotesCommandHandler(new FakeAppointmentRepository(appointment), new FakeTimeZoneProvider(), unitOfWork);

        var result = await handler.HandleAsync(new UpdateAppointmentInternalNotesCommand(appointment.BusinessId, appointment.Id, new string('x', 1001)), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(UpdateAppointmentInternalNotesError.InternalNotesTooLong);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    private static Appointment CreateAppointment(Guid businessId, AppointmentStatus status)
    {
        var business = new Business
        {
            Id = businessId,
            Name = "Barberia Centro",
            Slug = "barberia-centro",
            TimeZoneId = "Europe/Madrid",
            CurrencyCode = "EUR",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var service = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Corte",
            DurationMinutes = 30,
            PriceAmount = 18m,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var staffMember = new StaffMember
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            DisplayName = "Ana",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "clara@example.test",
            NormalizedEmail = "CLARA@EXAMPLE.TEST",
            FirstName = "Clara",
            PasswordHash = "hash",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var startAtUtc = new DateTimeOffset(2026, 7, 20, 8, 0, 0, TimeSpan.Zero);

        return new Appointment
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Business = business,
            ServiceId = service.Id,
            Service = service,
            StaffMemberId = staffMember.Id,
            StaffMember = staffMember,
            CustomerId = customer.Id,
            Customer = customer,
            StartAtUtc = startAtUtc,
            EndAtUtc = startAtUtc.AddMinutes(30),
            Status = status,
            ServiceNameSnapshot = service.Name,
            ServiceDurationMinutesSnapshot = service.DurationMinutes,
            PriceAmountSnapshot = service.PriceAmount,
            CurrencyCodeSnapshot = business.CurrencyCode,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private sealed class FakeAppointmentRepository(Appointment? appointment) : IAppointmentRepository
    {
        public Task<Appointment?> GetByIdWithDetailsForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(appointment?.Id == id ? appointment : null);

        public Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Appointment>> ListByCustomerIdWithDetailsAsync(Guid customerId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, AppointmentStatus? status, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Appointment>> ListByBusinessIdWithDetailsAsync(Guid businessId, DateTimeOffset fromUtc, DateTimeOffset toUtc, Guid? staffMemberId, Guid? serviceId, AppointmentStatus? status, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Appointment>> ListBlockingAppointmentsAsync(Guid businessId, IReadOnlyCollection<Guid> staffMemberIds, DateTimeOffset rangeStartUtc, DateTimeOffset rangeEndUtc, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasBlockingOverlapAsync(Guid businessId, Guid staffMemberId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, Guid? excludedAppointmentId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(Appointment appointment) => throw new NotSupportedException();
    }

    private sealed class FakeTimeZoneProvider : ITimeZoneProvider
    {
        public bool TryGetIanaTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo)
        {
            timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(timeZoneId, TimeSpan.FromHours(2), timeZoneId, timeZoneId);
            return true;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }
    }
}
