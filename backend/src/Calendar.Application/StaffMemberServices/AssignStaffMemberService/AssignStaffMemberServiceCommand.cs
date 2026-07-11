using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberServices.AssignStaffMemberService;

public sealed record AssignStaffMemberServiceCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    Guid ServiceId) : ICommand<AssignStaffMemberServiceResult>;
