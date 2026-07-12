using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberServices.UnassignStaffMemberService;

public sealed record UnassignStaffMemberServiceCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    Guid ServiceId) : ICommand<UnassignStaffMemberServiceResult>;
