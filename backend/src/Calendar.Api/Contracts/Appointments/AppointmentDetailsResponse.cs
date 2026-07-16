namespace Calendar.Api.Contracts.Appointments;

/// <summary>Represents detailed appointment information returned by the API.</summary>
public sealed record AppointmentDetailsResponse(
    Guid Id,
    AppointmentBusinessResponse Business,
    AppointmentServiceResponse Service,
    AppointmentStaffMemberResponse StaffMember,
    AppointmentCustomerResponse Customer,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    DateOnly LocalDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    string? CustomerNotes,
    DateTimeOffset? CancelledAtUtc,
    string? CancellationReason,
    DateTimeOffset CreatedAtUtc);

/// <summary>Represents the business attached to an appointment.</summary>
public sealed record AppointmentBusinessResponse(Guid Id, string Name, string Slug, string TimeZoneId);

/// <summary>Represents the service snapshot attached to an appointment.</summary>
public sealed record AppointmentServiceResponse(
    Guid Id,
    string NameSnapshot,
    int DurationMinutesSnapshot,
    decimal PriceAmountSnapshot,
    string CurrencyCodeSnapshot);

/// <summary>Represents the staff member attached to an appointment.</summary>
public sealed record AppointmentStaffMemberResponse(Guid Id, string DisplayName);

/// <summary>Represents the customer attached to an appointment.</summary>
public sealed record AppointmentCustomerResponse(Guid Id, string FirstName, string? LastName, string Email);
