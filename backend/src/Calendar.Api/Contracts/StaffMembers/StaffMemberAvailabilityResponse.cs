namespace Calendar.Api.Contracts.StaffMembers;

/// <summary>Represents a staff member weekly availability block.</summary>
public sealed record StaffMemberAvailabilityResponse(
    Guid Id,
    Guid StaffMemberId,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
