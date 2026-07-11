using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMembers.CreateStaffMember;

public sealed record CreateStaffMemberCommand(
    Guid BusinessId,
    string DisplayName,
    string? Email,
    string? PhoneNumber,
    string? Bio,
    int SortOrder) : ICommand<CreateStaffMemberResult>;
