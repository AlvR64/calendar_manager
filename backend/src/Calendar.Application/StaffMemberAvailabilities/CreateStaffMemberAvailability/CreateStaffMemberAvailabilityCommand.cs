using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberAvailabilities.CreateStaffMemberAvailability;

public sealed record CreateStaffMemberAvailabilityCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime) : ICommand<CreateStaffMemberAvailabilityResult>;
