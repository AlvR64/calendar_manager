using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(CalendarDbContext dbContext) : ICustomerRepository
{
    public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Customers.AnyAsync(customer => customer.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<Customer?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Customers.FirstOrDefaultAsync(customer => customer.NormalizedEmail == normalizedEmail, cancellationToken);

    public void Add(Customer customer) => dbContext.Customers.Add(customer);
}
