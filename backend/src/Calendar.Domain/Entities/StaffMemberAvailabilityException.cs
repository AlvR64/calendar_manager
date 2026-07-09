namespace Calendar.Domain.Entities;

public sealed class StaffMemberAvailabilityException
{
    public Guid Id { get; set; }
    public Guid StaffMemberId { get; set; }
    public DateOnly LocalDate { get; set; }
    public bool IsClosed { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }

    public StaffMember StaffMember { get; set; } = null!;
}
