using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IAdminRepository
{
    Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    void Add(Admin admin);
}
