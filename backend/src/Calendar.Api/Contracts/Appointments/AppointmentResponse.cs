namespace Calendar.Api.Contracts.Appointments;

/// <summary>Represents an appointment returned by the API.</summary>
public sealed record AppointmentResponse(
    Guid Id,
    Guid BusinessId,
    Guid StaffMemberId,
    Guid ServiceId,
    Guid CustomerId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    string Status,
    string? CustomerNotes,
    string ServiceNameSnapshot,
    int ServiceDurationMinutesSnapshot,
    decimal PriceAmountSnapshot,
    string CurrencyCodeSnapshot,
    DateTimeOffset CreatedAtUtc);
