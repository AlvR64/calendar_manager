namespace Calendar.Api.Contracts.Businesses;

/// <summary>Represents an available appointment slot for a staff member.</summary>
public sealed record AvailableSlotResponse(
    Guid StaffMemberId,
    DateOnly LocalDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc);
