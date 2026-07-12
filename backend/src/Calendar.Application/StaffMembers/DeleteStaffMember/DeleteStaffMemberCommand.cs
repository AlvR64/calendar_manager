using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMembers.DeleteStaffMember;

public sealed record DeleteStaffMemberCommand(Guid BusinessId, Guid StaffMemberId) : ICommand<DeleteStaffMemberResult>;
