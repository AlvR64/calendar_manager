using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IBusinessRepository
{
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsActiveByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<Business?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Business?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken);

    void Add(Business business);
}
