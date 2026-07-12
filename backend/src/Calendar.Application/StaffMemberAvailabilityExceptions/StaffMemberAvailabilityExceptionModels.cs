namespace Calendar.Application.StaffMemberAvailabilityExceptions;

public sealed record StaffMemberAvailabilityExceptionDetails(
    Guid Id,
    Guid StaffMemberId,
    DateOnly LocalDate,
    bool IsClosed,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason,
    DateTimeOffset CreatedAtUtc);

public sealed record ListStaffMemberAvailabilityExceptionsResult(
    bool Succeeded,
    ListStaffMemberAvailabilityExceptionsError Error,
    IReadOnlyList<StaffMemberAvailabilityExceptionDetails> Exceptions)
{
    public static ListStaffMemberAvailabilityExceptionsResult Success(IReadOnlyList<StaffMemberAvailabilityExceptionDetails> exceptions) => new(
        true,
        ListStaffMemberAvailabilityExceptionsError.None,
        exceptions);

    public static ListStaffMemberAvailabilityExceptionsResult Failure(ListStaffMemberAvailabilityExceptionsError error) => new(false, error, []);
}

public enum ListStaffMemberAvailabilityExceptionsError
{
    None = 0,
    StaffMemberNotFound = 1
}
