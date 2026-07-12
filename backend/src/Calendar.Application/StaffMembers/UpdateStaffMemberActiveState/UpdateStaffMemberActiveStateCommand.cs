using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMembers.UpdateStaffMemberActiveState;

public sealed record UpdateStaffMemberActiveStateCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    bool IsActive) : ICommand<UpdateStaffMemberActiveStateResult>;
