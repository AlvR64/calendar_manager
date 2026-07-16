using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Appointments;

/// <summary>Payload for updating appointment internal notes as an admin.</summary>
public sealed record UpdateAppointmentInternalNotesRequest
{
    /// <summary>Internal notes visible only to admins.</summary>
    [MaxLength(1000)]
    public string? InternalNotes { get; init; }
}
