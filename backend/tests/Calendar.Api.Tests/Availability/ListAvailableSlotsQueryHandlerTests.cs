using Calendar.Application.Availability;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Availability;

public sealed class ListAvailableSlotsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenWeeklyAvailabilityExists_ReturnsGeneratedSlots()
    {
        var fixture = CreateFixture();
        fixture.Availabilities.Add(new StaffMemberAvailability
        {
            StaffMemberId = fixture.StaffMemberId,
            DayOfWeek = fixture.LocalDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true
        });

        var result = await fixture.Handler.HandleAsync(fixture.Query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Slots.Should().HaveCount(3);
        result.Slots.Select(slot => slot.StartTime).Should().Equal(
            new TimeOnly(9, 0),
            new TimeOnly(9, 15),
            new TimeOnly(9, 30));
    }

    [Fact]
    public async Task HandleAsync_WhenClosedExceptionExists_ReturnsNoSlots()
    {
        var fixture = CreateFixture();
        fixture.Availabilities.Add(new StaffMemberAvailability
        {
            StaffMemberId = fixture.StaffMemberId,
            DayOfWeek = fixture.LocalDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true
        });
        fixture.Exceptions.Add(new StaffMemberAvailabilityException
        {
            StaffMemberId = fixture.StaffMemberId,
            LocalDate = fixture.LocalDate,
            IsClosed = true
        });

        var result = await fixture.Handler.HandleAsync(fixture.Query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenAppointmentStartsAtSlotEnd_DoesNotBlockPreviousSlot()
    {
        var fixture = CreateFixture();
        fixture.Availabilities.Add(new StaffMemberAvailability
        {
            StaffMemberId = fixture.StaffMemberId,
            DayOfWeek = fixture.LocalDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true
        });
        fixture.Appointments.Add(new Appointment
        {
            BusinessId = fixture.BusinessId,
            StaffMemberId = fixture.StaffMemberId,
            ServiceId = fixture.ServiceId,
            StartAtUtc = new DateTimeOffset(fixture.LocalDate.Year, fixture.LocalDate.Month, fixture.LocalDate.Day, 9, 30, 0, TimeSpan.Zero),
            EndAtUtc = new DateTimeOffset(fixture.LocalDate.Year, fixture.LocalDate.Month, fixture.LocalDate.Day, 10, 0, 0, TimeSpan.Zero),
            Status = AppointmentStatus.Scheduled
        });

        var result = await fixture.Handler.HandleAsync(fixture.Query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Slots.Should().ContainSingle();
        result.Slots[0].StartTime.Should().Be(new TimeOnly(9, 0));
        result.Slots[0].EndTime.Should().Be(new TimeOnly(9, 30));
    }

    private static Fixture CreateFixture()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var localDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var business = new Business
        {
            Id = businessId,
            TimeZoneId = "Europe/Madrid",
            MaxAdvanceBookingDays = 60,
            IsActive = true
        };
        var service = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            DurationMinutes = 30,
            IsActive = true
        };
        var staffMember = new StaffMember
        {
            Id = staffMemberId,
            BusinessId = businessId,
            IsActive = true
        };
        var assignment = new StaffMemberService
        {
            StaffMemberId = staffMemberId,
            ServiceId = serviceId,
            IsActive = true
        };

        var businessRepository = new FakeBusinessRepository { Business = business };
        var serviceRepository = new FakeServiceRepository { Service = service };
        var staffMemberRepository = new FakeStaffMemberRepository { StaffMember = staffMember };
        var staffMemberServiceRepository = new FakeStaffMemberServiceRepository { Assignments = [assignment] };
        var availabilityRepository = new FakeStaffMemberAvailabilityRepository();
        var exceptionRepository = new FakeStaffMemberAvailabilityExceptionRepository();
        var appointmentRepository = new FakeAppointmentRepository();

        var handler = new ListAvailableSlotsQueryHandler(
            businessRepository,
            serviceRepository,
            staffMemberRepository,
            staffMemberServiceRepository,
            availabilityRepository,
            exceptionRepository,
            appointmentRepository,
            new UtcTimeZoneProvider());

        return new Fixture(
            businessId,
            serviceId,
            staffMemberId,
            localDate,
            new ListAvailableSlotsQuery(businessId, serviceId, staffMemberId, localDate),
            handler,
            availabilityRepository.Availabilities,
            exceptionRepository.Exceptions,
            appointmentRepository.Appointments);
    }

    private sealed record Fixture(
        Guid BusinessId,
        Guid ServiceId,
        Guid StaffMemberId,
        DateOnly LocalDate,
        ListAvailableSlotsQuery Query,
        ListAvailableSlotsQueryHandler Handler,
        List<StaffMemberAvailability> Availabilities,
        List<StaffMemberAvailabilityException> Exceptions,
        List<Appointment> Appointments);

    private sealed class UtcTimeZoneProvider : ITimeZoneProvider
    {
        public bool TryGetIanaTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo)
        {
            timeZoneInfo = TimeZoneInfo.Utc;
            return timeZoneId == "Europe/Madrid";
        }
    }

    private sealed class FakeBusinessRepository : IBusinessRepository
    {
        public Business? Business { get; init; }

        public Task<Business?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Business?.Id == id ? Business : null);

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ExistsActiveByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Business?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Business?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(Business business) => throw new NotSupportedException();
    }

    private sealed class FakeServiceRepository : IServiceRepository
    {
        public Service? Service { get; init; }

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

    private sealed class FakeStaffMemberRepository : IStaffMemberRepository
    {
        public StaffMember? StaffMember { get; init; }

        public Task<StaffMember?> GetActiveByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) =>
            Task.FromResult(StaffMember?.Id == id && StaffMember.BusinessId == businessId && StaffMember.IsActive ? StaffMember : null);

        public Task<bool> ExistsByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<StaffMember>> ListByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<StaffMember?> GetByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<StaffMember?> GetByIdAndBusinessIdForUpdateAsync(Guid id, Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<StaffMember>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasAppointmentsAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(StaffMember staffMember) => throw new NotSupportedException();

        public void Remove(StaffMember staffMember) => throw new NotSupportedException();
    }

    private sealed class FakeStaffMemberServiceRepository : IStaffMemberServiceRepository
    {
        public IReadOnlyList<StaffMemberService> Assignments { get; init; } = [];

        public Task<bool> ExistsActiveAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) =>
            Task.FromResult(Assignments.Any(assignment => assignment.StaffMemberId == staffMemberId && assignment.ServiceId == serviceId && assignment.IsActive));

        public Task<IReadOnlyList<StaffMemberService>> ListActiveByBusinessIdAndServiceIdAsync(Guid businessId, Guid serviceId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<StaffMemberService>>(Assignments.Where(assignment => assignment.ServiceId == serviceId && assignment.IsActive).ToList());

        public Task<bool> ExistsAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<StaffMemberService?> GetByIdsForUpdateAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasAppointmentsAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<StaffMemberService>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(StaffMemberService staffMemberService) => throw new NotSupportedException();

        public void Remove(StaffMemberService staffMemberService) => throw new NotSupportedException();
    }

    private sealed class FakeStaffMemberAvailabilityRepository : IStaffMemberAvailabilityRepository
    {
        public List<StaffMemberAvailability> Availabilities { get; } = [];

        public Task<IReadOnlyList<StaffMemberAvailability>> ListActiveByStaffMemberIdsAndDayAsync(
            IReadOnlyCollection<Guid> staffMemberIds,
            DayOfWeek dayOfWeek,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<StaffMemberAvailability>>(Availabilities
                .Where(availability => staffMemberIds.Contains(availability.StaffMemberId)
                    && availability.DayOfWeek == dayOfWeek
                    && availability.IsActive)
                .ToList());

        public Task<IReadOnlyList<StaffMemberAvailability>> ListByStaffMemberIdAsync(Guid staffMemberId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<StaffMemberAvailability?> GetByIdAndStaffMemberIdForUpdateAsync(Guid id, Guid staffMemberId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> OverlapsAsync(Guid staffMemberId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, Guid? excludedAvailabilityId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(StaffMemberAvailability availability) => throw new NotSupportedException();

        public void Remove(StaffMemberAvailability availability) => throw new NotSupportedException();
    }

    private sealed class FakeStaffMemberAvailabilityExceptionRepository : IStaffMemberAvailabilityExceptionRepository
    {
        public List<StaffMemberAvailabilityException> Exceptions { get; } = [];

        public Task<IReadOnlyList<StaffMemberAvailabilityException>> ListByStaffMemberIdsAndDateAsync(
            IReadOnlyCollection<Guid> staffMemberIds,
            DateOnly localDate,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<StaffMemberAvailabilityException>>(Exceptions
                .Where(exception => staffMemberIds.Contains(exception.StaffMemberId) && exception.LocalDate == localDate)
                .ToList());

        public Task<IReadOnlyList<StaffMemberAvailabilityException>> ListByStaffMemberIdAsync(Guid staffMemberId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<StaffMemberAvailabilityException?> GetByIdAndStaffMemberIdForUpdateAsync(Guid id, Guid staffMemberId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasAnyExceptionForDateAsync(Guid staffMemberId, DateOnly localDate, Guid? excludedExceptionId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasClosedExceptionAsync(Guid staffMemberId, DateOnly localDate, Guid? excludedExceptionId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> OverlapsAsync(Guid staffMemberId, DateOnly localDate, TimeOnly startTime, TimeOnly endTime, Guid? excludedExceptionId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Add(StaffMemberAvailabilityException exception) => throw new NotSupportedException();

        public void Remove(StaffMemberAvailabilityException exception) => throw new NotSupportedException();
    }

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        public List<Appointment> Appointments { get; } = [];

        public Task<IReadOnlyList<Appointment>> ListBlockingAppointmentsAsync(
            Guid businessId,
            IReadOnlyCollection<Guid> staffMemberIds,
            DateTimeOffset rangeStartUtc,
            DateTimeOffset rangeEndUtc,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Appointment>>(Appointments
                .Where(appointment => appointment.BusinessId == businessId
                    && staffMemberIds.Contains(appointment.StaffMemberId)
                    && appointment.Status != AppointmentStatus.CancelledByAdmin
                    && appointment.Status != AppointmentStatus.CancelledByCustomer
                    && appointment.StartAtUtc < rangeEndUtc
                    && rangeStartUtc < appointment.EndAtUtc)
                .ToList());
    }
}
