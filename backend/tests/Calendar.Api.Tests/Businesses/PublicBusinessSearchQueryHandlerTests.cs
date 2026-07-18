using Calendar.Application.Businesses;

namespace Calendar.Api.Tests.Businesses;

public sealed class PublicBusinessSearchQueryHandlerTests
{
    [Theory]
    [InlineData(0, 12)]
    [InlineData(1, 0)]
    [InlineData(1, PublicBusinessSearchQueryHandler.MaxPageSize + 1)]
    public async Task HandleAsync_WhenPaginationIsInvalid_ReturnsInvalidPagination(int page, int pageSize)
    {
        var repository = new FakePublicBusinessSearchRepository();
        var handler = new PublicBusinessSearchQueryHandler(repository);

        var result = await handler.HandleAsync(new SearchPublicBusinessesQuery(null, null, null, null, page, pageSize), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(PublicBusinessSearchError.InvalidPagination);
        repository.Criteria.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_NormalizesFiltersBeforeSearching()
    {
        var repository = new FakePublicBusinessSearchRepository();
        var handler = new PublicBusinessSearchQueryHandler(repository);

        var result = await handler.HandleAsync(
            new SearchPublicBusinessesQuery("  barber  ", "   ", " Barberia ", " corte ", 1, 12),
            CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        repository.Criteria.Should().Be(new PublicBusinessSearchCriteria("barber", null, "Barberia", "corte", 1, 12));
    }

    private sealed class FakePublicBusinessSearchRepository : IPublicBusinessSearchRepository
    {
        public PublicBusinessSearchCriteria? Criteria { get; private set; }

        public Task<PublicBusinessSearchPage> SearchAsync(PublicBusinessSearchCriteria criteria, CancellationToken cancellationToken)
        {
            Criteria = criteria;
            return Task.FromResult(new PublicBusinessSearchPage([], criteria.Page, criteria.PageSize, 0, HasNextPage: false));
        }
    }
}
