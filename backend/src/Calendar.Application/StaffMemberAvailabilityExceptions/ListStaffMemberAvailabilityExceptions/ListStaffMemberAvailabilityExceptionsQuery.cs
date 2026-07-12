using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.ListStaffMemberAvailabilityExceptions;

public sealed record ListStaffMemberAvailabilityExceptionsQuery(
    Guid BusinessId,
    Guid StaffMemberId) : IQuery<ListStaffMemberAvailabilityExceptionsResult>;
