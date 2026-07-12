using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IStaffMemberAvailabilityExceptionRepository
{
    Task<IReadOnlyList<StaffMemberAvailabilityException>> ListByStaffMemberIdAsync(
        Guid staffMemberId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StaffMemberAvailabilityException>> ListByStaffMemberIdsAndDateAsync(
        IReadOnlyCollection<Guid> staffMemberIds,
        DateOnly localDate,
        CancellationToken cancellationToken);

    Task<StaffMemberAvailabilityException?> GetByIdAndStaffMemberIdForUpdateAsync(
        Guid id,
        Guid staffMemberId,
        CancellationToken cancellationToken);

    Task<bool> HasAnyExceptionForDateAsync(
        Guid staffMemberId,
        DateOnly localDate,
        Guid? excludedExceptionId,
        CancellationToken cancellationToken);

    Task<bool> HasClosedExceptionAsync(
        Guid staffMemberId,
        DateOnly localDate,
        Guid? excludedExceptionId,
        CancellationToken cancellationToken);

    Task<bool> OverlapsAsync(
        Guid staffMemberId,
        DateOnly localDate,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludedExceptionId,
        CancellationToken cancellationToken);

    void Add(StaffMemberAvailabilityException exception);

    void Remove(StaffMemberAvailabilityException exception);
}
