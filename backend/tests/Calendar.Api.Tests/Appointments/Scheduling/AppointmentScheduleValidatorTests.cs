using Calendar.Application.Appointments.Scheduling;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Appointments.Scheduling;

public sealed class AppointmentScheduleValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenAppointmentFitsWeeklyAvailability_ReturnsSuccess()
    {
        var fixture = CreateFixture();

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Error.Should().Be(AppointmentScheduleValidationError.None);
    }

    [Fact]
    public async Task ValidateAsync_WhenAppointmentIsOutsideWeeklyAvailability_ReturnsOutsideAvailability()
    {
        var fixture = CreateFixture(startTime: new TimeOnly(8, 30), endTime: new TimeOnly(9, 0));

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.OutsideAvailability);
    }

    [Fact]
    public async Task ValidateAsync_WhenWeeklyAvailabilityDoesNotExist_ReturnsOutsideAvailability()
    {
        var fixture = CreateFixture(addDefaultAvailability: false);

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.OutsideAvailability);
    }

    [Fact]
    public async Task ValidateAsync_WhenClosedExceptionExists_ReturnsOutsideAvailability()
    {
        var fixture = CreateFixture();
        fixture.Exceptions.Add(new StaffMemberAvailabilityException
        {
            StaffMemberId = fixture.StaffMemberId,
            LocalDate = fixture.LocalDate,
            IsClosed = true
        });

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.OutsideAvailability);
    }

    [Fact]
    public async Task ValidateAsync_WhenAppointmentFitsOpenException_ReturnsSuccess()
    {
        var fixture = CreateFixture(startTime: new TimeOnly(13, 0), endTime: new TimeOnly(13, 30));
        fixture.Exceptions.Add(new StaffMemberAvailabilityException
        {
            StaffMemberId = fixture.StaffMemberId,
            LocalDate = fixture.LocalDate,
            IsClosed = false,
            StartTime = new TimeOnly(13, 0),
            EndTime = new TimeOnly(14, 0)
        });

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Error.Should().Be(AppointmentScheduleValidationError.None);
    }

    [Fact]
    public async Task ValidateAsync_WhenOpenExceptionExistsButAppointmentDoesNotFit_ReturnsOutsideAvailability()
    {
        var fixture = CreateFixture(startTime: new TimeOnly(10, 0), endTime: new TimeOnly(10, 30));
        fixture.Exceptions.Add(new StaffMemberAvailabilityException
        {
            StaffMemberId = fixture.StaffMemberId,
            LocalDate = fixture.LocalDate,
            IsClosed = false,
            StartTime = new TimeOnly(13, 0),
            EndTime = new TimeOnly(14, 0)
        });

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.OutsideAvailability);
    }

    [Fact]
    public async Task ValidateAsync_WhenAppointmentCrossesLocalDate_ReturnsOutsideAvailability()
    {
        var fixture = CreateFixture(startTime: new TimeOnly(23, 45), endTime: new TimeOnly(23, 59));
        var request = fixture.Request with
        {
            EndAtUtc = new DateTimeOffset(fixture.LocalDate.AddDays(1).ToDateTime(new TimeOnly(0, 15), DateTimeKind.Utc), TimeSpan.Zero)
        };

        var result = await fixture.Validator.ValidateAsync(request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.OutsideAvailability);
    }

    [Fact]
    public async Task ValidateAsync_WhenAppointmentIsBeforeToday_ReturnsOutsideBookingWindow()
    {
        var fixture = CreateFixture(localDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.OutsideBookingWindow);
    }

    [Fact]
    public async Task ValidateAsync_WhenAppointmentExceedsMaxAdvanceBookingDays_ReturnsOutsideBookingWindow()
    {
        var fixture = CreateFixture(localDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));
        fixture.Business.MaxAdvanceBookingDays = 1;

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.OutsideBookingWindow);
    }

    [Fact]
    public async Task ValidateAsync_WhenBusinessTimeZoneIsInvalid_ReturnsInvalidBusinessTimeZone()
    {
        var fixture = CreateFixture();
        fixture.Business.TimeZoneId = "Invalid/Zone";

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.InvalidBusinessTimeZone);
    }

    [Fact]
    public async Task ValidateAsync_WhenBusinessDoesNotExist_ReturnsBusinessNotFound()
    {
        var fixture = CreateFixture();
        fixture.BusinessRepository.Business = null;

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.BusinessNotFound);
    }

    [Fact]
    public async Task ValidateAsync_WhenServiceDoesNotExist_ReturnsServiceNotFound()
    {
        var fixture = CreateFixture();
        fixture.ServiceRepository.Service = null;

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.ServiceNotFound);
    }

    [Fact]
    public async Task ValidateAsync_WhenStaffMemberDoesNotExist_ReturnsStaffMemberNotFound()
    {
        var fixture = CreateFixture();
        fixture.StaffMemberRepository.StaffMember = null;

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.StaffMemberNotFound);
    }

    [Fact]
    public async Task ValidateAsync_WhenAssignmentDoesNotExist_ReturnsStaffMemberServiceAssignmentNotFound()
    {
        var fixture = CreateFixture();
        fixture.Assignments.Clear();

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.StaffMemberServiceAssignmentNotFound);
    }

    [Fact]
    public async Task ValidateAsync_WhenTimeRangeIsInvalid_ReturnsInvalidTimeRange()
    {
        var fixture = CreateFixture();
        var request = fixture.Request with { EndAtUtc = fixture.Request.StartAtUtc };

        var result = await fixture.Validator.ValidateAsync(request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.InvalidTimeRange);
    }

    [Theory]
    [InlineData(AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task ValidateAsync_WhenBlockingAppointmentOverlaps_ReturnsAppointmentOverlaps(AppointmentStatus status)
    {
        var fixture = CreateFixture();
        fixture.Appointments.Add(CreateAppointment(fixture, status, fixture.Request.StartAtUtc.AddMinutes(15), fixture.Request.EndAtUtc.AddMinutes(15)));

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(AppointmentScheduleValidationError.AppointmentOverlaps);
    }

    [Theory]
    [InlineData(AppointmentStatus.CancelledByCustomer)]
    [InlineData(AppointmentStatus.CancelledByAdmin)]
    public async Task ValidateAsync_WhenCancelledAppointmentOverlaps_ReturnsSuccess(AppointmentStatus status)
    {
        var fixture = CreateFixture();
        fixture.Appointments.Add(CreateAppointment(fixture, status, fixture.Request.StartAtUtc.AddMinutes(15), fixture.Request.EndAtUtc.AddMinutes(15)));

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Error.Should().Be(AppointmentScheduleValidationError.None);
    }

    [Fact]
    public async Task ValidateAsync_WhenAppointmentEndsAtExistingStart_ReturnsSuccess()
    {
        var fixture = CreateFixture();
        fixture.Appointments.Add(CreateAppointment(fixture, AppointmentStatus.Scheduled, fixture.Request.EndAtUtc, fixture.Request.EndAtUtc.AddMinutes(30)));

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Error.Should().Be(AppointmentScheduleValidationError.None);
    }

    [Fact]
    public async Task ValidateAsync_WhenAppointmentStartsAtExistingEnd_ReturnsSuccess()
    {
        var fixture = CreateFixture();
        fixture.Appointments.Add(CreateAppointment(fixture, AppointmentStatus.Scheduled, fixture.Request.StartAtUtc.AddMinutes(-30), fixture.Request.StartAtUtc));

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Error.Should().Be(AppointmentScheduleValidationError.None);
    }

    [Fact]
    public async Task ValidateAsync_WhenOverlappingAppointmentIsExcluded_ReturnsSuccess()
    {
        var fixture = CreateFixture();
        var appointmentId = Guid.NewGuid();
        fixture.Appointments.Add(CreateAppointment(fixture, AppointmentStatus.Scheduled, fixture.Request.StartAtUtc, fixture.Request.EndAtUtc, appointmentId));
        var request = fixture.Request with { ExcludedAppointmentId = appointmentId };

        var result = await fixture.Validator.ValidateAsync(request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Error.Should().Be(AppointmentScheduleValidationError.None);
    }

    [Fact]
    public async Task ValidateAsync_WhenOverlappingAppointmentBelongsToOtherStaffMember_ReturnsSuccess()
    {
        var fixture = CreateFixture();
        var appointment = CreateAppointment(fixture, AppointmentStatus.Scheduled, fixture.Request.StartAtUtc, fixture.Request.EndAtUtc);
        appointment.StaffMemberId = Guid.NewGuid();
        fixture.Appointments.Add(appointment);

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Error.Should().Be(AppointmentScheduleValidationError.None);
    }

    [Fact]
    public async Task ValidateAsync_WhenOverlappingAppointmentBelongsToOtherBusiness_ReturnsSuccess()
    {
        var fixture = CreateFixture();
        var appointment = CreateAppointment(fixture, AppointmentStatus.Scheduled, fixture.Request.StartAtUtc, fixture.Request.EndAtUtc);
        appointment.BusinessId = Guid.NewGuid();
        fixture.Appointments.Add(appointment);

        var result = await fixture.Validator.ValidateAsync(fixture.Request, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Error.Should().Be(AppointmentScheduleValidationError.None);
    }

    private static Appointment CreateAppointment(
        Fixture fixture,
        AppointmentStatus status,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid? appointmentId = null) =>
        new()
        {
            Id = appointmentId ?? Guid.NewGuid(),
            BusinessId = fixture.BusinessId,
            StaffMemberId = fixture.StaffMemberId,
            ServiceId = fixture.ServiceId,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Status = status
        };

    private static Fixture CreateFixture(
        DateOnly? localDate = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        bool addDefaultAvailability = true)
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var date = localDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var appointmentStartTime = startTime ?? new TimeOnly(10, 0);
        var appointmentEndTime = endTime ?? new TimeOnly(10, 30);
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
        var availabilityExceptionRepository = new FakeStaffMemberAvailabilityExceptionRepository();
        var appointmentRepository = new FakeAppointmentRepository();

        if (addDefaultAvailability)
        {
            availabilityRepository.Availabilities.Add(new StaffMemberAvailability
            {
                StaffMemberId = staffMemberId,
                DayOfWeek = date.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = true
            });
        }

        var validator = new AppointmentScheduleValidator(
            businessRepository,
            serviceRepository,
            staffMemberRepository,
            staffMemberServiceRepository,
            availabilityRepository,
            availabilityExceptionRepository,
            appointmentRepository,
            new UtcTimeZoneProvider());

        var request = new AppointmentScheduleValidationRequest(
            businessId,
            serviceId,
            staffMemberId,
            new DateTimeOffset(date.ToDateTime(appointmentStartTime, DateTimeKind.Utc), TimeSpan.Zero),
            new DateTimeOffset(date.ToDateTime(appointmentEndTime, DateTimeKind.Utc), TimeSpan.Zero));

        return new Fixture(
            businessId,
            serviceId,
            staffMemberId,
            date,
            business,
            request,
            validator,
            businessRepository,
            serviceRepository,
            staffMemberRepository,
            staffMemberServiceRepository.Assignments,
            availabilityRepository.Availabilities,
            availabilityExceptionRepository.Exceptions,
            appointmentRepository.Appointments);
    }

    private sealed record Fixture(
        Guid BusinessId,
        Guid ServiceId,
        Guid StaffMemberId,
        DateOnly LocalDate,
        Business Business,
        AppointmentScheduleValidationRequest Request,
        AppointmentScheduleValidator Validator,
        FakeBusinessRepository BusinessRepository,
        FakeServiceRepository ServiceRepository,
        FakeStaffMemberRepository StaffMemberRepository,
        List<StaffMemberService> Assignments,
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
        public Business? Business { get; set; }

        public Task<Business?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Business?.Id == id && Business.IsActive ? Business : null);

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

    private sealed class FakeStaffMemberRepository : IStaffMemberRepository
    {
        public StaffMember? StaffMember { get; set; }

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
        public List<StaffMemberService> Assignments { get; init; } = [];

        public Task<bool> ExistsActiveAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) =>
            Task.FromResult(Assignments.Any(assignment => assignment.StaffMemberId == staffMemberId && assignment.ServiceId == serviceId && assignment.IsActive));

        public Task<bool> ExistsAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<StaffMemberService?> GetByIdsForUpdateAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> HasAppointmentsAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<StaffMemberService>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<StaffMemberService>> ListActiveByBusinessIdAndServiceIdAsync(Guid businessId, Guid serviceId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<StaffMemberService>> ListByBusinessIdAndStaffMemberIdAsync(Guid businessId, Guid staffMemberId, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<StaffMemberService>> ListByBusinessIdAndServiceIdAsync(Guid businessId, Guid serviceId, CancellationToken cancellationToken) => throw new NotSupportedException();

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

        public Task<bool> HasBlockingOverlapAsync(
            Guid businessId,
            Guid staffMemberId,
            DateTimeOffset startAtUtc,
            DateTimeOffset endAtUtc,
            Guid? excludedAppointmentId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Appointments.Any(appointment => appointment.BusinessId == businessId
                && appointment.StaffMemberId == staffMemberId
                && appointment.Status != AppointmentStatus.CancelledByCustomer
                && appointment.Status != AppointmentStatus.CancelledByAdmin
                && (!excludedAppointmentId.HasValue || appointment.Id != excludedAppointmentId.Value)
                && appointment.StartAtUtc < endAtUtc
                && startAtUtc < appointment.EndAtUtc));

        public Task<IReadOnlyList<Appointment>> ListBlockingAppointmentsAsync(
            Guid businessId,
            IReadOnlyCollection<Guid> staffMemberIds,
            DateTimeOffset rangeStartUtc,
            DateTimeOffset rangeEndUtc,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
