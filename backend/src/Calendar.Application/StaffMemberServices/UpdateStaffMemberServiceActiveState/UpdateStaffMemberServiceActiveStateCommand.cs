using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberServices.UpdateStaffMemberServiceActiveState;

public sealed record UpdateStaffMemberServiceActiveStateCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    Guid ServiceId,
    bool IsActive) : ICommand<UpdateStaffMemberServiceActiveStateResult>;
