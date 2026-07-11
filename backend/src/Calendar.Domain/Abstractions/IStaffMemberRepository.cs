using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IStaffMemberRepository
{
    Task<bool> ExistsByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    void Add(StaffMember staffMember);
}
