namespace Calendar.Application.Businesses;

public sealed record PublicBusinessSearchCriteria(
    string? Query,
    string? City,
    string? Category,
    string? Service,
    int Page,
    int PageSize)
{
    public bool HasFilters => Query is not null || City is not null || Category is not null || Service is not null;
}

public interface IPublicBusinessSearchRepository
{
    Task<PublicBusinessSearchPage> SearchAsync(PublicBusinessSearchCriteria criteria, CancellationToken cancellationToken);
}
