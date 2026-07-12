namespace Calendar.Api.Contracts.StaffMembers;

/// <summary>Represents a staff member availability exception on a specific date.</summary>
public sealed record StaffMemberAvailabilityExceptionResponse(
    Guid Id,
    Guid StaffMemberId,
    DateOnly LocalDate,
    bool IsClosed,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason,
    DateTimeOffset CreatedAtUtc);
