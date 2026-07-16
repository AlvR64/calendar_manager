namespace Calendar.Application.Appointments.GetAppointmentDetails;

public sealed record AppointmentDetails(
    Guid Id,
    AppointmentBusinessDetails Business,
    AppointmentServiceDetails Service,
    AppointmentStaffMemberDetails StaffMember,
    AppointmentCustomerDetails Customer,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    DateOnly LocalDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    string? CustomerNotes,
    string? InternalNotes,
    DateTimeOffset? CancelledAtUtc,
    string? CancellationReason,
    DateTimeOffset CreatedAtUtc);

public sealed record AppointmentBusinessDetails(Guid Id, string Name, string Slug, string TimeZoneId);

public sealed record AppointmentServiceDetails(
    Guid Id,
    string NameSnapshot,
    int DurationMinutesSnapshot,
    decimal PriceAmountSnapshot,
    string CurrencyCodeSnapshot);

public sealed record AppointmentStaffMemberDetails(Guid Id, string DisplayName);

public sealed record AppointmentCustomerDetails(Guid Id, string FirstName, string? LastName, string Email);
