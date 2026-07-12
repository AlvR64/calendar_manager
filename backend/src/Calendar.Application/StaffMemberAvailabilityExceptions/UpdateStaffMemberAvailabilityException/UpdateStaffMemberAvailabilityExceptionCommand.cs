using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.UpdateStaffMemberAvailabilityException;

public sealed record UpdateStaffMemberAvailabilityExceptionCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    Guid ExceptionId,
    DateOnly LocalDate,
    bool IsClosed,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason) : ICommand<UpdateStaffMemberAvailabilityExceptionResult>;
