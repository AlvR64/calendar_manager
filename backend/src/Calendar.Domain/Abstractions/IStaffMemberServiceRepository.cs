using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IStaffMemberServiceRepository
{
    Task<bool> ExistsAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken);

    Task<bool> ExistsActiveAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken);

    Task<StaffMemberService?> GetByIdsForUpdateAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken);

    Task<bool> HasAppointmentsAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StaffMemberService>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StaffMemberService>> ListActiveByBusinessIdAndServiceIdAsync(
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken);

    void Add(StaffMemberService staffMemberService);

    void Remove(StaffMemberService staffMemberService);
}
