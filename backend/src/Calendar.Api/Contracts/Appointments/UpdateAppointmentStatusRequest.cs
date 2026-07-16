using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Appointments;

/// <summary>Payload for changing an appointment operational status as an admin.</summary>
public sealed record UpdateAppointmentStatusRequest
{
    /// <summary>The new operational appointment status.</summary>
    [Required]
    public required string Status { get; init; }
}
