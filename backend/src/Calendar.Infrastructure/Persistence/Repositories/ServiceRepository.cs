using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class ServiceRepository(CalendarDbContext dbContext) : IServiceRepository
{
    public void Add(Service service) => dbContext.Services.Add(service);
}
