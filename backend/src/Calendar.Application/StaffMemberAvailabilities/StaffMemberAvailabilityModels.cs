namespace Calendar.Application.StaffMemberAvailabilities;

public sealed record StaffMemberAvailabilityDetails(
    Guid Id,
    Guid StaffMemberId,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);

public sealed record ListStaffMemberAvailabilitiesResult(
    bool Succeeded,
    ListStaffMemberAvailabilitiesError Error,
    IReadOnlyList<StaffMemberAvailabilityDetails> Availabilities)
{
    public static ListStaffMemberAvailabilitiesResult Success(IReadOnlyList<StaffMemberAvailabilityDetails> availabilities) => new(
        true,
        ListStaffMemberAvailabilitiesError.None,
        availabilities);

    public static ListStaffMemberAvailabilitiesResult Failure(ListStaffMemberAvailabilitiesError error) => new(false, error, []);
}

public enum ListStaffMemberAvailabilitiesError
{
    None = 0,
    StaffMemberNotFound = 1
}
