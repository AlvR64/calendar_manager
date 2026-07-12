using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class BusinessRepository(CalendarDbContext dbContext) : IBusinessRepository
{
    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Businesses.AnyAsync(business => business.Id == id, cancellationToken);

    public Task<bool> ExistsActiveByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Businesses.AnyAsync(
            business => business.Id == id && business.IsActive,
            cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Businesses.AnyAsync(business => business.Slug == slug, cancellationToken);

    public Task<Business?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(business => business.Id == id && business.IsActive, cancellationToken);

    public Task<Business?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(business => business.Slug == slug && business.IsActive, cancellationToken);

    public Task<Business?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Businesses.FirstOrDefaultAsync(business => business.Id == id, cancellationToken);

    public void Add(Business business) => dbContext.Businesses.Add(business);
}
