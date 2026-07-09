namespace Calendar.Domain.Entities;

public sealed class Appointment
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid StaffMemberId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset StartAtUtc { get; set; }
    public DateTimeOffset EndAtUtc { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? CustomerNotes { get; set; }
    public string? InternalNotes { get; set; }
    public string ServiceNameSnapshot { get; set; } = string.Empty;
    public int ServiceDurationMinutesSnapshot { get; set; }
    public decimal PriceAmountSnapshot { get; set; }
    public string CurrencyCodeSnapshot { get; set; } = string.Empty;
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public Business Business { get; set; } = null!;
    public StaffMember StaffMember { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
