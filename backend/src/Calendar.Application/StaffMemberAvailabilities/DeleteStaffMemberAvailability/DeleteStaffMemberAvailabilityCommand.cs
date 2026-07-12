using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberAvailabilities.DeleteStaffMemberAvailability;

public sealed record DeleteStaffMemberAvailabilityCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    Guid AvailabilityId) : ICommand<DeleteStaffMemberAvailabilityResult>;
