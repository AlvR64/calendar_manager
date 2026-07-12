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

    public async Task<IReadOnlyList<Service>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) =>
        await dbContext.Services
            .AsNoTracking()
            .Where(service => service.BusinessId == businessId && service.IsActive)
            .OrderBy(service => service.SortOrder)
            .ThenBy(service => service.Name)
            .ToListAsync(cancellationToken);

    public Task<Service?> GetActiveByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) =>
        dbContext.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(
                service => service.Id == id
                    && service.BusinessId == businessId
                    && service.IsActive
                    && service.Business.IsActive,
                cancellationToken);

    public void Add(Service service) => dbContext.Services.Add(service);
}
