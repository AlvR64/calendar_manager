using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.DeleteStaffMemberAvailabilityException;

public sealed record DeleteStaffMemberAvailabilityExceptionCommand(
    Guid BusinessId,
    Guid StaffMemberId,
    Guid ExceptionId) : ICommand<DeleteStaffMemberAvailabilityExceptionResult>;
