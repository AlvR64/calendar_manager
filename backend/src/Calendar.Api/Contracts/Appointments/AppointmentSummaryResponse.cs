namespace Calendar.Api.Contracts.Appointments;

/// <summary>Represents an appointment summary returned in customer lists.</summary>
public sealed record AppointmentSummaryResponse(
    Guid Id,
    AppointmentBusinessResponse Business,
    AppointmentServiceResponse Service,
    AppointmentStaffMemberResponse StaffMember,
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
