using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class BusinessRepository(CalendarDbContext dbContext) : IBusinessRepository
{
    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Businesses.AnyAsync(business => business.Slug == slug, cancellationToken);

    public void Add(Business business) => dbContext.Businesses.Add(business);
}
