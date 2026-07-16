namespace Calendar.Api.Contracts.Appointments;

/// <summary>Represents an appointment summary returned in appointment lists.</summary>
public sealed record AppointmentSummaryResponse(
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
    string? InternalNotes,
    DateTimeOffset? CancelledAtUtc,
    string? CancellationReason,
    DateTimeOffset CreatedAtUtc);
