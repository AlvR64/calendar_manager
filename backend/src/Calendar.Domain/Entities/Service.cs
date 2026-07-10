namespace Calendar.Domain.Entities;

public sealed class Service
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal PriceAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public Business Business { get; set; } = null!;
    public ICollection<StaffMemberService> StaffMemberServices { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
