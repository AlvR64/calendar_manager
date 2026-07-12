using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMembers.UpdateStaffMember;

public sealed record UpdateStaffMemberCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    string DisplayName,
    string? Email,
    string? PhoneNumber,
    string? Bio,
    int SortOrder) : ICommand<UpdateStaffMemberResult>;
