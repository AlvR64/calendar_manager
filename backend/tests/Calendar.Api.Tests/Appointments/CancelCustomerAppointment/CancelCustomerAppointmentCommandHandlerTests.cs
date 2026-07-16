using Calendar.Application.Appointments.CancelCustomerAppointment;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Appointments.CancelCustomerAppointment;

public sealed class CancelCustomerAppointmentCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenAppointmentIsFutureAndScheduled_CancelsAppointment()
    {
        var customerId = Guid.NewGuid();
        var appointment = CreateAppointment(customerId, DateTimeOffset.UtcNow.AddDays(2), AppointmentStatus.Scheduled);
        var repository = new FakeAppointmentRepository { Appointment = appointment };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelCustomerAppointmentCommandHandler(repository, new FakeTimeZoneProvider(TimeSpan.Zero), unitOfWork);

        var result = await handler.HandleAsync(new CancelCustomerAppointmentCommand(customerId, appointment.Id, "  No puedo ir  "), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.CancelledByCustomer);
        appointment.CancelledAtUtc.Should().NotBeNull();
        appointment.CancellationReason.Should().Be("No puedo ir");
        appointment.UpdatedAtUtc.Should().Be(appointment.CancelledAtUtc);
        result.Appointment!.Status.Should().Be(nameof(AppointmentStatus.CancelledByCustomer));
        result.Appointment.CancellationReason.Should().Be("No puedo ir");
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenAppointmentBelongsToAnotherCustomer_ReturnsForbidden()
    {
        var appointment = CreateAppointment(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(2), AppointmentStatus.Scheduled);
        var handler = CreateHandler(appointment, out var unitOfWork);

        var result = await handler.HandleAsync(new CancelCustomerAppointmentCommand(Guid.NewGuid(), appointment.Id, null), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(CancelCustomerAppointmentError.Forbidden);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Theory]
    [InlineData(AppointmentStatus.CancelledByCustomer, CancelCustomerAppointmentError.AlreadyCancelled)]
    [InlineData(AppointmentStatus.Completed, CancelCustomerAppointmentError.AppointmentNotCancelable)]
    public async Task HandleAsync_WhenAppointmentStatusIsNotCancelable_ReturnsExpectedError(AppointmentStatus status, CancelCustomerAppointmentError expectedError)
    {
        var customerId = Guid.NewGuid();
        var appointment = CreateAppointment(customerId, DateTimeOffset.UtcNow.AddDays(2), status);
        var handler = CreateHandler(appointment, out var unitOfWork);

        var result = await handler.HandleAsync(new CancelCustomerAppointmentCommand(customerId, appointment.Id, null), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(expectedError);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenAppointmentIsInPast_ReturnsAppointmentInPast()
    {
        var customerId = Guid.NewGuid();
        var appointment = CreateAppointment(customerId, DateTimeOffset.UtcNow.AddHours(-1), AppointmentStatus.Scheduled);
        var handler = CreateHandler(appointment, out var unitOfWork);

        var result = await handler.HandleAsync(new CancelCustomerAppointmentCommand(customerId, appointment.Id, null), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(CancelCustomerAppointmentError.AppointmentInPast);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    private static CancelCustomerAppointmentCommandHandler CreateHandler(Appointment? appointment, out FakeUnitOfWork unitOfWork)
    {
        unitOfWork = new FakeUnitOfWork();
        return new CancelCustomerAppointmentCommandHandler(
            new FakeAppointmentRepository { Appointment = appointment },
            new FakeTimeZoneProvider(TimeSpan.Zero),
            unitOfWork);
    }

    private static Appointment CreateAppointment(Guid customerId, DateTimeOffset startAtUtc, AppointmentStatus status)
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = "Barberia Centro",
            Slug = "barberia-centro",
            TimeZoneId = "Europe/Madrid",
            CurrencyCode = "EUR",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var service = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "Corte",
            DurationMinutes = 30,
            PriceAmount = 18m,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var staffMember = new StaffMember
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            DisplayName = "Ana",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var customer = new Customer
        {
            Id = customerId,
            Email = "clara@example.test",
            NormalizedEmail = "CLARA@EXAMPLE.TEST",
            FirstName = "Clara",
            PasswordHash = "hash",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        return new Appointment
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Business = business,
            ServiceId = service.Id,
            Service = service,
            StaffMemberId = staffMember.Id,
            StaffMember = staffMember,
            CustomerId = customerId,
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

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        public Appointment? Appointment { get; init; }

        public Task<Appointment?> GetByIdWithDetailsForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Appointment?.Id == id ? Appointment : null);

        public Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Appointment>> ListByCustomerIdWithDetailsAsync(Guid customerId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, AppointmentStatus? status, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Appointment>> ListBlockingAppointmentsAsync(Guid businessId, IReadOnlyCollection<Guid> staffMemberIds, DateTimeOffset rangeStartUtc, DateTimeOffset rangeEndUtc, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasBlockingOverlapAsync(Guid businessId, Guid staffMemberId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, Guid? excludedAppointmentId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(Appointment appointment) => throw new NotSupportedException();
    }

    private sealed class FakeTimeZoneProvider(TimeSpan offset) : ITimeZoneProvider
    {
        public bool TryGetIanaTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo)
        {
            timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(timeZoneId, offset, timeZoneId, timeZoneId);
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
