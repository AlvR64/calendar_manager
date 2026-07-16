using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Appointments;

/// <summary>Payload for cancelling an appointment as a customer.</summary>
public sealed record CancelAppointmentRequest
{
    /// <summary>Optional cancellation reason visible from the customer.</summary>
    [MaxLength(500)]
    public string? CancellationReason { get; init; }
}
