namespace Calendar.Api.Contracts.Services;

/// <summary>Payload for updating a service active state.</summary>
public sealed record UpdateServiceActiveStateRequest
{
    /// <summary>Whether the service is active.</summary>
    public bool IsActive { get; init; }
}
