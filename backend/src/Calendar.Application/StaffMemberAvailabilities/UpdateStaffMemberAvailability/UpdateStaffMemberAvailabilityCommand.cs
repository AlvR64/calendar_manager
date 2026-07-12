using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberAvailabilities.UpdateStaffMemberAvailability;

public sealed record UpdateStaffMemberAvailabilityCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    Guid AvailabilityId,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime) : ICommand<UpdateStaffMemberAvailabilityResult>;
