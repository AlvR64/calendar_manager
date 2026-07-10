using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IBusinessRepository
{
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);

    void Add(Business business);
}
