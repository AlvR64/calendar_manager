using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface ICustomerRepository
{
    Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    void Add(Customer customer);
}
