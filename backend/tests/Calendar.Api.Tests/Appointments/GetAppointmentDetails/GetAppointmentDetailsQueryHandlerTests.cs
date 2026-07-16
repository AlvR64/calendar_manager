using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Appointments.GetAppointmentDetails;

public sealed class GetAppointmentDetailsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenAppointmentExists_ReturnsDetailsWithBusinessLocalTimes()
    {
        var appointment = CreateAppointment();
        var repository = new FakeAppointmentRepository { Appointment = appointment };
        var timeZoneProvider = new FakeTimeZoneProvider(TimeSpan.FromHours(2));
        var handler = new GetAppointmentDetailsQueryHandler(repository, timeZoneProvider);

        var result = await handler.HandleAsync(new GetAppointmentDetailsQuery(appointment.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
        result.Business.Id.Should().Be(appointment.BusinessId);
        result.Business.Name.Should().Be("Barberia Centro");
        result.Service.NameSnapshot.Should().Be("Corte");
        result.Service.DurationMinutesSnapshot.Should().Be(30);
        result.StaffMember.DisplayName.Should().Be("Ana");
        result.Customer.Email.Should().Be("clara@example.test");
        result.StartAtUtc.Should().Be(appointment.StartAtUtc);
        result.EndAtUtc.Should().Be(appointment.EndAtUtc);
        result.LocalDate.Should().Be(new DateOnly(2026, 7, 20));
        result.StartTime.Should().Be(new TimeOnly(10, 0));
        result.EndTime.Should().Be(new TimeOnly(10, 30));
        result.Status.Should().Be("Scheduled");
        result.CustomerNotes.Should().Be("Notas");
    }

    [Fact]
    public async Task HandleAsync_WhenAppointmentDoesNotExist_ReturnsNull()
    {
        var repository = new FakeAppointmentRepository();
        var handler = new GetAppointmentDetailsQueryHandler(repository, new FakeTimeZoneProvider(TimeSpan.Zero));

        var result = await handler.HandleAsync(new GetAppointmentDetailsQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    private static Appointment CreateAppointment()
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
            Name = "Current service name",
            DurationMinutes = 45,
            PriceAmount = 25m,
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
            Id = Guid.NewGuid(),
            Email = "clara@example.test",
            NormalizedEmail = "CLARA@EXAMPLE.TEST",
            FirstName = "Clara",
            LastName = "Diaz",
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
            StartAtUtc = new DateTimeOffset(2026, 7, 20, 8, 0, 0, TimeSpan.Zero),
            EndAtUtc = new DateTimeOffset(2026, 7, 20, 8, 30, 0, TimeSpan.Zero),
            Status = AppointmentStatus.Scheduled,
            CustomerNotes = "Notas",
            ServiceNameSnapshot = "Corte",
            ServiceDurationMinutesSnapshot = 30,
            PriceAmountSnapshot = 18m,
            CurrencyCodeSnapshot = "EUR",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        public Appointment? Appointment { get; init; }

        public Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Appointment?.Id == id ? Appointment : null);

        public Task<Appointment?> GetByIdWithDetailsForUpdateAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Appointment>> ListByCustomerIdWithDetailsAsync(Guid customerId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, AppointmentStatus? status, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Appointment>> ListByBusinessIdWithDetailsAsync(Guid businessId, DateTimeOffset fromUtc, DateTimeOffset toUtc, Guid? staffMemberId, Guid? serviceId, AppointmentStatus? status, CancellationToken cancellationToken) => throw new NotSupportedException();

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
