namespace Calendar.Application.Availability;

public sealed record AvailableSlotDetails(
    Guid StaffMemberId,
    DateOnly LocalDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc);

public sealed record ListAvailableSlotsResult(
    bool Succeeded,
    ListAvailableSlotsError Error,
    IReadOnlyList<AvailableSlotDetails> Slots)
{
    public static ListAvailableSlotsResult Success(IReadOnlyList<AvailableSlotDetails> slots) => new(
        true,
        ListAvailableSlotsError.None,
        slots);

    public static ListAvailableSlotsResult Failure(ListAvailableSlotsError error) => new(false, error, []);
}

public enum ListAvailableSlotsError
{
    None = 0,
    BusinessNotFound = 1,
    ServiceNotFound = 2,
    StaffMemberNotFound = 3,
    StaffMemberServiceAssignmentNotFound = 4,
    InvalidDate = 5,
    InvalidBusinessTimeZone = 6
}
