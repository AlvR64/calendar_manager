using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IStaffMemberServiceRepository
{
    Task<bool> ExistsAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StaffMemberService>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);

    void Add(StaffMemberService staffMemberService);
}
