using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IStaffMemberAvailabilityExceptionRepository
{
    Task<IReadOnlyList<StaffMemberAvailabilityException>> ListByStaffMemberIdAsync(
        Guid staffMemberId,
        CancellationToken cancellationToken);

    Task<bool> HasAnyExceptionForDateAsync(
        Guid staffMemberId,
        DateOnly localDate,
        CancellationToken cancellationToken);

    Task<bool> HasClosedExceptionAsync(
        Guid staffMemberId,
        DateOnly localDate,
        CancellationToken cancellationToken);

    Task<bool> OverlapsAsync(
        Guid staffMemberId,
        DateOnly localDate,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken);

    void Add(StaffMemberAvailabilityException exception);
}
