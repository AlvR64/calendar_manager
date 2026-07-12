using System.ComponentModel.DataAnnotations;

namespace Calendar.Api.Contracts.Businesses;

/// <summary>Payload for updating the current admin business booking window.</summary>
public sealed record UpdateBusinessBookingWindowRequest
{
    /// <summary>The maximum number of days in advance customers can book.</summary>
    [Range(1, 365)]
    public required int MaxAdvanceBookingDays { get; init; }
}
