using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.StaffMembers;

/// <summary>Payload for updating a staff member weekly availability block.</summary>
public sealed record UpdateStaffMemberAvailabilityRequest
{
    /// <summary>The day of week, where 0 is Sunday and 6 is Saturday.</summary>
    [Range(0, 6)]
    public int DayOfWeek { get; init; }

    /// <summary>The local start time.</summary>
    public TimeOnly StartTime { get; init; }

    /// <summary>The local end time.</summary>
    public TimeOnly EndTime { get; init; }
}
