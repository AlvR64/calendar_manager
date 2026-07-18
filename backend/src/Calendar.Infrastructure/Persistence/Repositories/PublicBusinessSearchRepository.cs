using Calendar.Application.Businesses;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class PublicBusinessSearchRepository(CalendarDbContext dbContext) : IPublicBusinessSearchRepository
{
    private const int FeaturedServiceCount = 3;

    public async Task<PublicBusinessSearchPage> SearchAsync(
        PublicBusinessSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Businesses
            .AsNoTracking()
            .Where(business => business.IsActive);

        if (criteria.Query is not null)
        {
            var searchTerm = criteria.Query;
            query = query.Where(business =>
                business.Name.Contains(searchTerm)
                || (business.Description != null && business.Description.Contains(searchTerm))
                || (business.City != null && business.City.Contains(searchTerm))
                || (business.Category != null && business.Category.Contains(searchTerm))
                || business.Services.Any(service =>
                    service.IsActive
                    && (service.Name.Contains(searchTerm)
                        || (service.Description != null && service.Description.Contains(searchTerm)))));
        }

        if (criteria.City is not null)
        {
            var city = criteria.City;
            query = query.Where(business => business.City != null && business.City.Contains(city));
        }

        if (criteria.Category is not null)
        {
            var category = criteria.Category;
            query = query.Where(business => business.Category != null && business.Category == category);
        }

        if (criteria.Service is not null)
        {
            var serviceTerm = criteria.Service;
            query = query.Where(business => business.Services.Any(service =>
                service.IsActive
                && (service.Name.Contains(serviceTerm)
                    || (service.Description != null && service.Description.Contains(serviceTerm)))));
        }

        query = criteria.HasFilters
            ? query.OrderBy(business => business.Name).ThenBy(business => business.Slug)
            : query.OrderByDescending(business => business.CreatedAtUtc).ThenBy(business => business.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (criteria.Page - 1) * criteria.PageSize;
        var items = await query
            .Skip(skip)
            .Take(criteria.PageSize)
            .Select(business => new PublicBusinessCardDetails(
                business.Id,
                business.Slug,
                business.Name,
                business.Description,
                business.City,
                business.CountryCode,
                business.Category,
                business.TimeZoneId,
                business.CurrencyCode,
                business.Services
                    .Where(service => service.IsActive)
                    .OrderBy(service => service.SortOrder)
                    .ThenBy(service => service.Name)
                    .Take(FeaturedServiceCount)
                    .Select(service => new PublicBusinessFeaturedServiceDetails(
                        service.Id,
                        service.Name,
                        service.DurationMinutes,
                        service.PriceAmount))
                    .ToList(),
                business.Services
                    .Where(service => service.IsActive)
                    .OrderBy(service => service.PriceAmount)
                    .Select(service => (decimal?)service.PriceAmount)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        var hasNextPage = skip + items.Count < totalCount;
        return new PublicBusinessSearchPage(items, criteria.Page, criteria.PageSize, totalCount, hasNextPage);
    }
}
