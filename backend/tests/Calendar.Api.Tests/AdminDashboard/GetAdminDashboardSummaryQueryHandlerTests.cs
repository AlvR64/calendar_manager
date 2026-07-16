using Calendar.Application.AdminDashboard.GetAdminDashboardSummary;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.AdminDashboard;

public sealed class GetAdminDashboardSummaryQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsCountsRevenueAndUpcomingAppointments()
    {
        var businessId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var repository = new FakeAppointmentRepository
        {
            Appointments =
            [
                CreateAppointment(businessId, today.ToDateTime(new TimeOnly(10, 0)), AppointmentStatus.Scheduled, 18m),
                CreateAppointment(businessId, today.AddDays(1).ToDateTime(new TimeOnly(10, 0)), AppointmentStatus.Scheduled, 18m),
                CreateAppointment(businessId, today.AddDays(2).ToDateTime(new TimeOnly(10, 0)), AppointmentStatus.CancelledByCustomer, 100m),
                CreateAppointment(Guid.NewGuid(), today.AddDays(1).ToDateTime(new TimeOnly(10, 0)), AppointmentStatus.Scheduled, 50m)
            ]
        };
        var handler = new GetAdminDashboardSummaryQueryHandler(
            new FakeBusinessRepository(CreateBusiness(businessId)),
            repository,
            new FakeTimeZoneProvider());

        var result = await handler.HandleAsync(new GetAdminDashboardSummaryQuery(businessId, today, today.AddDays(7)), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Summary!.TodayAppointmentCount.Should().Be(1);
        result.Summary.EstimatedRevenueAmount.Should().Be(36m);
        result.Summary.CurrencyCode.Should().Be("EUR");
        result.Summary.UpcomingAppointments.Should().Contain(appointment => appointment.Status == nameof(AppointmentStatus.Scheduled));
        result.Summary.UpcomingAppointments.Should().NotContain(appointment => appointment.Status == nameof(AppointmentStatus.CancelledByCustomer));
        result.Summary.StatusCounts.Should().ContainEquivalentOf(new AdminDashboardStatusCount(nameof(AppointmentStatus.Scheduled), 2));
        result.Summary.StatusCounts.Should().ContainEquivalentOf(new AdminDashboardStatusCount(nameof(AppointmentStatus.CancelledByCustomer), 1));
    }

    [Fact]
    public async Task HandleAsync_WhenRangeIsInvalid_ReturnsInvalidDateRange()
    {
        var businessId = Guid.NewGuid();
        var handler = new GetAdminDashboardSummaryQueryHandler(
            new FakeBusinessRepository(CreateBusiness(businessId)),
            new FakeAppointmentRepository(),
            new FakeTimeZoneProvider());

        var result = await handler.HandleAsync(new GetAdminDashboardSummaryQuery(businessId, new DateOnly(2026, 7, 20), new DateOnly(2026, 7, 19)), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(GetAdminDashboardSummaryError.InvalidDateRange);
    }

    private static Business CreateBusiness(Guid businessId) => new()
    {
        Id = businessId,
        Name = "Barberia Centro",
        Slug = "barberia-centro",
        TimeZoneId = "UTC",
        CurrencyCode = "EUR",
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private static Appointment CreateAppointment(Guid businessId, DateTime localStart, AppointmentStatus status, decimal priceAmount)
    {
        var business = CreateBusiness(businessId);
        var service = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Corte",
            DurationMinutes = 30,
            PriceAmount = priceAmount,
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
        var startAtUtc = new DateTimeOffset(localStart, TimeSpan.Zero);

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
            PriceAmountSnapshot = priceAmount,
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

    private sealed class FakeTimeZoneProvider : ITimeZoneProvider
    {
        public bool TryGetIanaTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo)
        {
            timeZoneInfo = TimeZoneInfo.Utc;
            return true;
        }
    }
}
