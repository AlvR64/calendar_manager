using Calendar.Application.Appointments.ListCustomerAppointments;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Appointments.ListCustomerAppointments;

public sealed class ListCustomerAppointmentsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsCustomerAppointmentsWithLocalTimes()
    {
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var repository = new FakeAppointmentRepository
        {
            Appointments =
            [
                CreateAppointment(customerId, new DateTimeOffset(2026, 7, 20, 8, 0, 0, TimeSpan.Zero), AppointmentStatus.Scheduled),
                CreateAppointment(otherCustomerId, new DateTimeOffset(2026, 7, 21, 8, 0, 0, TimeSpan.Zero), AppointmentStatus.Scheduled),
                CreateAppointment(customerId, new DateTimeOffset(2026, 7, 22, 8, 0, 0, TimeSpan.Zero), AppointmentStatus.CancelledByCustomer)
            ]
        };
        var handler = new ListCustomerAppointmentsQueryHandler(repository, new FakeTimeZoneProvider(TimeSpan.FromHours(2)));

        var result = await handler.HandleAsync(new ListCustomerAppointmentsQuery(customerId, null, null, AppointmentStatus.Scheduled), CancellationToken.None);

        result.Should().ContainSingle();
        var appointment = result[0];
        appointment.Customer.Id.Should().Be(customerId);
        appointment.LocalDate.Should().Be(new DateOnly(2026, 7, 20));
        appointment.StartTime.Should().Be(new TimeOnly(10, 0));
        appointment.EndTime.Should().Be(new TimeOnly(10, 30));
        appointment.Status.Should().Be(nameof(AppointmentStatus.Scheduled));
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
        public IReadOnlyList<Appointment> Appointments { get; init; } = [];

        public Task<IReadOnlyList<Appointment>> ListByCustomerIdWithDetailsAsync(Guid customerId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, AppointmentStatus? status, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Appointment>>(Appointments
                .Where(appointment => appointment.CustomerId == customerId)
                .Where(appointment => !fromUtc.HasValue || appointment.EndAtUtc >= fromUtc.Value)
                .Where(appointment => !toUtc.HasValue || appointment.StartAtUtc <= toUtc.Value)
                .Where(appointment => !status.HasValue || appointment.Status == status.Value)
                .OrderBy(appointment => appointment.StartAtUtc)
                .ToList());

        public Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Appointment?> GetByIdWithDetailsForUpdateAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

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
