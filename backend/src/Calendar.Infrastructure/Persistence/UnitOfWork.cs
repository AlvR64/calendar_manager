using Calendar.Domain.Abstractions;

namespace Calendar.Infrastructure.Persistence;

public sealed class UnitOfWork(CalendarDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
