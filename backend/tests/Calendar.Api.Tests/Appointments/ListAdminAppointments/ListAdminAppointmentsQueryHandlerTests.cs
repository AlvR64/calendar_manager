using Calendar.Application.Appointments.ListAdminAppointments;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Appointments.ListAdminAppointments;

public sealed class ListAdminAppointmentsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsBusinessAppointmentsWithFiltersAndLocalTimes()
    {
        var businessId = Guid.NewGuid();
        var otherBusinessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var otherStaffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var otherServiceId = Guid.NewGuid();
        var repository = new FakeAppointmentRepository
        {
            Appointments =
            [
                CreateAppointment(businessId, staffMemberId, serviceId, new DateTimeOffset(2026, 7, 20, 8, 0, 0, TimeSpan.Zero), AppointmentStatus.Scheduled),
                CreateAppointment(businessId, otherStaffMemberId, serviceId, new DateTimeOffset(2026, 7, 20, 9, 0, 0, TimeSpan.Zero), AppointmentStatus.Scheduled),
                CreateAppointment(businessId, staffMemberId, otherServiceId, new DateTimeOffset(2026, 7, 20, 10, 0, 0, TimeSpan.Zero), AppointmentStatus.Scheduled),
                CreateAppointment(otherBusinessId, staffMemberId, serviceId, new DateTimeOffset(2026, 7, 20, 11, 0, 0, TimeSpan.Zero), AppointmentStatus.Scheduled)
            ]
        };
        var handler = new ListAdminAppointmentsQueryHandler(
            new FakeBusinessRepository(CreateBusiness(businessId)),
            repository,
            new FakeTimeZoneProvider(TimeSpan.FromHours(2)));

        var result = await handler.HandleAsync(
            new ListAdminAppointmentsQuery(
                businessId,
                new DateOnly(2026, 7, 20),
                new DateOnly(2026, 7, 20),
                staffMemberId,
                serviceId,
                AppointmentStatus.Scheduled),
            CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Appointments.Should().ContainSingle();
        var appointment = result.Appointments[0];
        appointment.Business.Id.Should().Be(businessId);
        appointment.StaffMember.Id.Should().Be(staffMemberId);
        appointment.Service.Id.Should().Be(serviceId);
        appointment.LocalDate.Should().Be(new DateOnly(2026, 7, 20));
        appointment.StartTime.Should().Be(new TimeOnly(10, 0));
    }

    [Fact]
    public async Task HandleAsync_WhenRangeIsInvalid_ReturnsInvalidDateRange()
    {
        var handler = new ListAdminAppointmentsQueryHandler(
            new FakeBusinessRepository(CreateBusiness(Guid.NewGuid())),
            new FakeAppointmentRepository(),
            new FakeTimeZoneProvider(TimeSpan.Zero));

        var result = await handler.HandleAsync(
            new ListAdminAppointmentsQuery(Guid.NewGuid(), new DateOnly(2026, 7, 21), new DateOnly(2026, 7, 20), null, null, null),
            CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(ListAdminAppointmentsError.InvalidDateRange);
    }

    [Fact]
    public async Task HandleAsync_WhenRangeIsTooLarge_ReturnsDateRangeTooLarge()
    {
        var handler = new ListAdminAppointmentsQueryHandler(
            new FakeBusinessRepository(CreateBusiness(Guid.NewGuid())),
            new FakeAppointmentRepository(),
            new FakeTimeZoneProvider(TimeSpan.Zero));

        var result = await handler.HandleAsync(
            new ListAdminAppointmentsQuery(Guid.NewGuid(), new DateOnly(2026, 7, 1), new DateOnly(2026, 10, 15), null, null, null),
            CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(ListAdminAppointmentsError.DateRangeTooLarge);
    }

    [Fact]
    public async Task HandleAsync_WhenBusinessDoesNotExist_ReturnsBusinessNotFound()
    {
        var handler = new ListAdminAppointmentsQueryHandler(
            new FakeBusinessRepository(null),
            new FakeAppointmentRepository(),
            new FakeTimeZoneProvider(TimeSpan.Zero));

        var result = await handler.HandleAsync(
            new ListAdminAppointmentsQuery(Guid.NewGuid(), new DateOnly(2026, 7, 20), new DateOnly(2026, 7, 20), null, null, null),
            CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(ListAdminAppointmentsError.BusinessNotFound);
    }

    private static Business CreateBusiness(Guid businessId) => new()
    {
        Id = businessId,
        Name = "Barberia Centro",
        Slug = "barberia-centro",
        TimeZoneId = "Europe/Madrid",
        CurrencyCode = "EUR",
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private static Appointment CreateAppointment(Guid businessId, Guid staffMemberId, Guid serviceId, DateTimeOffset startAtUtc, AppointmentStatus status)
    {
        var business = CreateBusiness(businessId);
        var service = new Service
        {
            Id = serviceId,
            BusinessId = business.Id,
            Name = "Corte",
            DurationMinutes = 30,
            PriceAmount = 18m,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var staffMember = new StaffMember
        {
            Id = staffMemberId,
            BusinessId = business.Id,
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

        return new Appointment
        {
            Id = Guid.NewGuid(),
            BusinessId = business.Id,
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

    private sealed class FakeBusinessRepository(Business? business) : IBusinessRepository
    {
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ExistsActiveByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Business?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Business?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Business?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(business?.Id == id ? business : null);

        public void Add(Business business) => throw new NotSupportedException();
    }

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        public IReadOnlyList<Appointment> Appointments { get; init; } = [];

        public Task<IReadOnlyList<Appointment>> ListByBusinessIdWithDetailsAsync(Guid businessId, DateTimeOffset fromUtc, DateTimeOffset toUtc, Guid? staffMemberId, Guid? serviceId, AppointmentStatus? status, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Appointment>>(Appointments
                .Where(appointment => appointment.BusinessId == businessId)
                .Where(appointment => appointment.StartAtUtc < toUtc && fromUtc < appointment.EndAtUtc)
                .Where(appointment => !staffMemberId.HasValue || appointment.StaffMemberId == staffMemberId.Value)
                .Where(appointment => !serviceId.HasValue || appointment.ServiceId == serviceId.Value)
                .Where(appointment => !status.HasValue || appointment.Status == status.Value)
                .OrderBy(appointment => appointment.StartAtUtc)
                .ToList());

        public Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Appointment?> GetByIdWithDetailsForUpdateAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

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
}
