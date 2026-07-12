using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.StaffMembers;

/// <summary>Payload for creating a staff member availability exception on a specific date.</summary>
public sealed record CreateStaffMemberAvailabilityExceptionRequest
{
    /// <summary>The local date affected by the exception.</summary>
    public DateOnly LocalDate { get; init; }

    /// <summary>Whether the staff member is closed for the entire date.</summary>
    public bool IsClosed { get; init; }

    /// <summary>The local start time for a special working block. Must be null when closed.</summary>
    public TimeOnly? StartTime { get; init; }

    /// <summary>The local end time for a special working block. Must be null when closed.</summary>
    public TimeOnly? EndTime { get; init; }

    /// <summary>Optional reason for the exception.</summary>
    [MaxLength(250)]
    public string? Reason { get; init; }
}
