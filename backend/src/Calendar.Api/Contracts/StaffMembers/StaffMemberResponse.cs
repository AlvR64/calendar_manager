namespace Calendar.Api.Contracts.StaffMembers;

/// <summary>Represents a staff member.</summary>
public sealed record StaffMemberResponse(
    Guid Id,
    Guid BusinessId,
    string DisplayName,
    string? Email,
    string? PhoneNumber,
    string? Bio,
    bool IsActive,
    int SortOrder,
    DateTimeOffset CreatedAtUtc);
