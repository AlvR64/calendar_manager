using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IServiceRepository
{
    Task<bool> ExistsByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Service>> ListByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);

    Task<Service?> GetByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Service>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);

    Task<Service?> GetActiveByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    void Add(Service service);
}
