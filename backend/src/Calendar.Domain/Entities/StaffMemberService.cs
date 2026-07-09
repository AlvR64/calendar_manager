namespace Calendar.Domain.Entities;

public sealed class StaffMemberService
{
    public Guid StaffMemberId { get; set; }
    public Guid ServiceId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }

    public StaffMember StaffMember { get; set; } = null!;
    public Service Service { get; set; } = null!;
}
