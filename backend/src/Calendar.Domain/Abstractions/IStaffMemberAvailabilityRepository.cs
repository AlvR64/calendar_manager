using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IStaffMemberAvailabilityRepository
{
    Task<IReadOnlyList<StaffMemberAvailability>> ListByStaffMemberIdAsync(Guid staffMemberId, CancellationToken cancellationToken);

    Task<StaffMemberAvailability?> GetByIdAndStaffMemberIdForUpdateAsync(
        Guid id,
        Guid staffMemberId,
        CancellationToken cancellationToken);

    Task<bool> OverlapsAsync(
        Guid staffMemberId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludedAvailabilityId,
        CancellationToken cancellationToken);

    void Add(StaffMemberAvailability availability);

    void Remove(StaffMemberAvailability availability);
}
