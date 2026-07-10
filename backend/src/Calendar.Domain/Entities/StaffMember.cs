namespace Calendar.Domain.Entities;

public sealed class StaffMember
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public Business Business { get; set; } = null!;
    public ICollection<StaffMemberService> StaffMemberServices { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<StaffMemberAvailability> Availabilities { get; set; } = [];
    public ICollection<StaffMemberAvailabilityException> AvailabilityExceptions { get; set; } = [];
}
