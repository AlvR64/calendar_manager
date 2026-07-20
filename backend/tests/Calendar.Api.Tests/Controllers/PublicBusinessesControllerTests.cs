using Calendar.Api.Contracts.Businesses;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Businesses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class PublicBusinessesControllerTests
{
    [Fact]
    public async Task SearchPublicBusinesses_WhenSearchSucceeds_ReturnsOkResponse()
    {
        var service = new PublicBusinessFeaturedServiceDetails(Guid.NewGuid(), "Corte de pelo", 30, 18m);
        var business = new PublicBusinessCardDetails(
            Guid.NewGuid(),
            "barberia-centro",
            "Barberia Centro",
            "Barberia de barrio",
            "Madrid",
            "ES",
            "barber",
            "Europe/Madrid",
            "EUR",
            [service],
            18m);
        var page = new PublicBusinessSearchPage([business], Page: 2, PageSize: 5, TotalCount: 8, HasNextPage: true);
        var handler = new StubQueryHandler(PublicBusinessSearchResult.Success(page));
        var controller = CreateController(handler);

        var result = await controller.SearchPublicBusinesses("barber", "Madrid", "barber", "Corte", 2, 5, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PublicBusinessSearchResponse>().Subject;
        response.Items.Should().ContainSingle();
        response.Items[0].Slug.Should().Be("barberia-centro");
        response.Items[0].FeaturedServices.Should().ContainSingle();
        response.Items[0].StartingPriceAmount.Should().Be(18m);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(5);
        response.TotalCount.Should().Be(8);
        response.HasNextPage.Should().BeTrue();
        handler.Query.Should().Be(new SearchPublicBusinessesQuery("barber", "Madrid", "barber", "Corte", 2, 5));
    }

    [Fact]
    public async Task SearchPublicBusinesses_WhenPaginationIsInvalid_ReturnsBadRequestProblemDetails()
    {
        var handler = new StubQueryHandler(PublicBusinessSearchResult.Failure(PublicBusinessSearchError.InvalidPagination));
        var controller = CreateController(handler, "/api/public/businesses");

        var result = await controller.SearchPublicBusinesses(null, null, null, null, 0, 12, CancellationToken.None);

        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Invalid pagination.");
        problemDetails.Instance.Should().Be("/api/public/businesses");
    }

    private static PublicBusinessesController CreateController(
        StubQueryHandler handler,
        string requestPath = "/api/public/businesses") => new(handler)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Path = requestPath
                }
            }
        }
    };

    private sealed class StubQueryHandler(PublicBusinessSearchResult result)
        : IQueryHandler<SearchPublicBusinessesQuery, PublicBusinessSearchResult>
    {
        public SearchPublicBusinessesQuery? Query { get; private set; }

        public Task<PublicBusinessSearchResult> HandleAsync(SearchPublicBusinessesQuery query, CancellationToken cancellationToken)
        {
            Query = query;
            return Task.FromResult(result);
        }
    }
}
