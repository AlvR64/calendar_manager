using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IServiceRepository
{
    Task<bool> ExistsByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken);

    void Add(Service service);
}
