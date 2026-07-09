namespace Calendar.Domain.Entities;

public sealed class StaffMemberAvailability
{
    public Guid Id { get; set; }
    public Guid StaffMemberId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }

    public StaffMember StaffMember { get; set; } = null!;
}
