using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Appointments;

/// <summary>Payload for creating an appointment as a customer.</summary>
public sealed record CreateAppointmentRequest
{
    /// <summary>The business where the appointment will be scheduled.</summary>
    public Guid BusinessId { get; init; }

    /// <summary>The service selected by the customer.</summary>
    public Guid ServiceId { get; init; }

    /// <summary>The staff member selected for the appointment.</summary>
    public Guid StaffMemberId { get; init; }

    /// <summary>The selected appointment start instant in UTC.</summary>
    public DateTimeOffset StartAtUtc { get; init; }

    /// <summary>Optional notes visible from the customer.</summary>
    [MaxLength(1000)]
    public string? CustomerNotes { get; init; }
}
