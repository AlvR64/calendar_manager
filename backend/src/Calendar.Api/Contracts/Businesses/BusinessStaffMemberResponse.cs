namespace Calendar.Api.Contracts.Businesses;

/// <summary>Represents a public staff member profile.</summary>
public sealed record BusinessStaffMemberResponse(
    Guid Id,
    string DisplayName,
    string? Bio,
    int SortOrder);
