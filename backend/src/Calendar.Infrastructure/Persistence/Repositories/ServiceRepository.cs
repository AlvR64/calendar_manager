using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class ServiceRepository(CalendarDbContext dbContext) : IServiceRepository
{
    public Task<bool> ExistsByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) =>
        dbContext.Services.AnyAsync(
            service => service.Id == id && service.BusinessId == businessId,
            cancellationToken);

    public void Add(Service service) => dbContext.Services.Add(service);
}
