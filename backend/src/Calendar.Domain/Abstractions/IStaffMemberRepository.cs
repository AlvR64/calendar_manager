using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IStaffMemberRepository
{
    Task<bool> ExistsByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StaffMember>> ListByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);

    Task<StaffMember?> GetByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    Task<StaffMember?> GetByIdAndBusinessIdForUpdateAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StaffMember>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);

    Task<StaffMember?> GetActiveByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    Task<bool> HasAppointmentsAsync(Guid id, CancellationToken cancellationToken);

    void Add(StaffMember staffMember);

    void Remove(StaffMember staffMember);
}
