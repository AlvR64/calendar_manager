using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Availability;

public sealed record ListAvailableSlotsQuery(
    Guid BusinessId,
    Guid ServiceId,
    Guid? StaffMemberId,
    DateOnly LocalDate) : IQuery<ListAvailableSlotsResult>;
