using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.CreateStaffMemberAvailabilityException;

public sealed record CreateStaffMemberAvailabilityExceptionCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    DateOnly LocalDate,
    bool IsClosed,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason) : ICommand<CreateStaffMemberAvailabilityExceptionResult>;
