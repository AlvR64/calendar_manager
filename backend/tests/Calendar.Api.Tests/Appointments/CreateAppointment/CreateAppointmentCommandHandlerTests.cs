using Calendar.Application.Appointments.CreateAppointment;
using Calendar.Application.Appointments.Scheduling;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Appointments.CreateAppointment;

public sealed class CreateAppointmentCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRequestIsValid_AddsAppointmentWithSnapshots()
    {
        var fixture = CreateFixture();
        var command = CreateCommand(fixture);

        var result = await fixture.Handler.HandleAsync(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.AppointmentId.Should().NotBeNull();
        result.BusinessId.Should().Be(fixture.Business.Id);
        result.ServiceId.Should().Be(fixture.Service.Id);
        result.StaffMemberId.Should().Be(fixture.StaffMember.Id);
        result.CustomerId.Should().Be(command.CustomerId);
        result.StartAtUtc.Should().Be(command.StartAtUtc.ToUniversalTime());
        result.EndAtUtc.Should().Be(command.StartAtUtc.ToUniversalTime().AddMinutes(fixture.Service.DurationMinutes));
        result.Status.Should().Be(nameof(AppointmentStatus.Scheduled));
        result.CustomerNotes.Should().Be("Necesito puntualidad");
        result.ServiceNameSnapshot.Should().Be(fixture.Service.Name);
        result.ServiceDurationMinutesSnapshot.Should().Be(fixture.Service.DurationMinutes);
        result.PriceAmountSnapshot.Should().Be(fixture.Service.PriceAmount);
        result.CurrencyCodeSnapshot.Should().Be(fixture.Business.CurrencyCode);

        fixture.AppointmentRepository.Appointments.Should().ContainSingle();
        var appointment = fixture.AppointmentRepository.Appointments[0];
        appointment.CustomerNotes.Should().Be("Necesito puntualidad");
        appointment.ServiceNameSnapshot.Should().Be(fixture.Service.Name);
        appointment.CurrencyCodeSnapshot.Should().Be(fixture.Business.CurrencyCode);
        fixture.UnitOfWork.SaveChangesCalls.Should().Be(1);
        fixture.ConcurrencyGuard.Resources.Should().ContainSingle()
            .Which.Should().StartWith($"appointment:{fixture.Business.Id}:{fixture.StaffMember.Id}:");
    }

    [Fact]
    public async Task HandleAsync_WhenBusinessDoesNotExist_ReturnsBusinessNotFoundAndDoesNotEnterLock()
    {
        var fixture = CreateFixture();
        fixture.BusinessRepository.Business = null;

        var result = await fixture.Handler.HandleAsync(CreateCommand(fixture), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(CreateAppointmentError.BusinessNotFound);
        fixture.ConcurrencyGuard.Resources.Should().BeEmpty();
        fixture.AppointmentRepository.Appointments.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenLockIsNotAcquired_ReturnsConcurrentSlotConflictAndDoesNotAddAppointment()
    {
        var fixture = CreateFixture();
        fixture.ConcurrencyGuard.AcquireLock = false;

        var result = await fixture.Handler.HandleAsync(CreateCommand(fixture), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(CreateAppointmentError.ConcurrentSlotConflict);
        fixture.AppointmentRepository.Appointments.Should().BeEmpty();
        fixture.UnitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduleValidatorReturnsOverlap_ReturnsAppointmentOverlapsAndDoesNotAddAppointment()
    {
        var fixture = CreateFixture();
        fixture.ScheduleValidator.Result = AppointmentScheduleValidationResult.Failure(AppointmentScheduleValidationError.AppointmentOverlaps);

        var result = await fixture.Handler.HandleAsync(CreateCommand(fixture), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(CreateAppointmentError.AppointmentOverlaps);
        fixture.AppointmentRepository.Appointments.Should().BeEmpty();
        fixture.UnitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceDoesNotExist_ReturnsServiceNotFoundAndDoesNotAddAppointment()
    {
        var fixture = CreateFixture();
        fixture.ServiceRepository.Service = null;

        var result = await fixture.Handler.HandleAsync(CreateCommand(fixture), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(CreateAppointmentError.ServiceNotFound);
        fixture.AppointmentRepository.Appointments.Should().BeEmpty();
        fixture.UnitOfWork.SaveChangesCalls.Should().Be(0);
    }

    private static CreateAppointmentCommand CreateCommand(Fixture fixture) => new(
        Guid.NewGuid(),
        fixture.Business.Id,
        fixture.Service.Id,
        fixture.StaffMember.Id,
        new DateTimeOffset(2026, 7, 20, 8, 0, 0, TimeSpan.Zero),
        "  Necesito puntualidad  ");

    private static Fixture CreateFixture()
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = "Barberia Centro",
            Slug = "barberia-centro",
            TimeZoneId = "Europe/Madrid",
            CurrencyCode = "EUR",
            MaxAdvanceBookingDays = 60,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var service = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            Name = "Corte",
            DurationMinutes = 30,
            PriceAmount = 18m,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var staffMember = new StaffMember
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
            DisplayName = "Ana",
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var businessRepository = new FakeBusinessRepository { Business = business };
        var serviceRepository = new FakeServiceRepository { Service = service };
        var appointmentRepository = new FakeAppointmentRepository();
        var scheduleValidator = new FakeScheduleValidator();
        var concurrencyGuard = new FakeAppointmentCreationConcurrencyGuard();
        var timeZoneProvider = new FakeTimeZoneProvider();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateAppointmentCommandHandler(
            businessRepository,
            serviceRepository,
            appointmentRepository,
            scheduleValidator,
            concurrencyGuard,
            timeZoneProvider,
            unitOfWork);

        return new Fixture(
            business,
            service,
            staffMember,
            businessRepository,
            serviceRepository,
            appointmentRepository,
            scheduleValidator,
            concurrencyGuard,
            unitOfWork,
            handler);
    }

    private sealed record Fixture(
        Business Business,
        Service Service,
        StaffMember StaffMember,
        FakeBusinessRepository BusinessRepository,
        FakeServiceRepository ServiceRepository,
        FakeAppointmentRepository AppointmentRepository,
        FakeScheduleValidator ScheduleValidator,
        FakeAppointmentCreationConcurrencyGuard ConcurrencyGuard,
        FakeUnitOfWork UnitOfWork,
        CreateAppointmentCommandHandler Handler);

    private sealed class FakeBusinessRepository : IBusinessRepository
    {
        public Business? Business { get; set; }

        public Task<Business?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Business?.Id == id && Business.IsActive ? Business : null);

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ExistsActiveByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Business?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Business?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(Business business) => throw new NotSupportedException();
    }

    private sealed class FakeServiceRepository : IServiceRepository
    {
        public Service? Service { get; set; }

        public Task<Service?> GetActiveByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) =>
            Task.FromResult(Service?.Id == id && Service.BusinessId == businessId && Service.IsActive ? Service : null);

        public Task<bool> ExistsByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Service>> ListByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Service?> GetByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Service?> GetByIdAndBusinessIdForUpdateAsync(Guid id, Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Service>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasAppointmentsAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(Service service) => throw new NotSupportedException();

        public void Remove(Service service) => throw new NotSupportedException();
    }

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        public List<Appointment> Appointments { get; } = [];

        public Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Appointment?> GetByIdWithDetailsForUpdateAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Appointment>> ListByCustomerIdWithDetailsAsync(Guid customerId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, AppointmentStatus? status, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(Appointment appointment) => Appointments.Add(appointment);

        public Task<IReadOnlyList<Appointment>> ListBlockingAppointmentsAsync(Guid businessId, IReadOnlyCollection<Guid> staffMemberIds, DateTimeOffset rangeStartUtc, DateTimeOffset rangeEndUtc, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasBlockingOverlapAsync(Guid businessId, Guid staffMemberId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, Guid? excludedAppointmentId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class FakeScheduleValidator : IAppointmentScheduleValidator
    {
        public AppointmentScheduleValidationResult Result { get; set; } = AppointmentScheduleValidationResult.Success();

        public Task<AppointmentScheduleValidationResult> ValidateAsync(AppointmentScheduleValidationRequest request, CancellationToken cancellationToken) => Task.FromResult(Result);
    }

    private sealed class FakeAppointmentCreationConcurrencyGuard : IAppointmentCreationConcurrencyGuard
    {
        public bool AcquireLock { get; set; } = true;

        public List<string> Resources { get; } = [];

        public Task<T> ExecuteWithLockAsync<T>(string resource, Func<CancellationToken, Task<T>> action, Func<T> lockNotAcquiredResult, CancellationToken cancellationToken)
        {
            Resources.Add(resource);
            return AcquireLock ? action(cancellationToken) : Task.FromResult(lockNotAcquiredResult());
        }
    }

    private sealed class FakeTimeZoneProvider : ITimeZoneProvider
    {
        public bool TryGetIanaTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo)
        {
            timeZoneInfo = TimeZoneInfo.Utc;
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
