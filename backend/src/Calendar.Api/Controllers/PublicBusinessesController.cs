using Calendar.Api.Contracts.Businesses;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Businesses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/businesses")]
public sealed class PublicBusinessesController(
    IQueryHandler<SearchPublicBusinessesQuery, PublicBusinessSearchResult> searchPublicBusinessesHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PublicBusinessSearchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PublicBusinessSearchResponse>> SearchPublicBusinesses(
        [FromQuery(Name = "query")] string? query,
        [FromQuery] string? city,
        [FromQuery] string? category,
        [FromQuery] string? service,
        [FromQuery] int page = PublicBusinessSearchQueryHandler.DefaultPage,
        [FromQuery] int pageSize = PublicBusinessSearchQueryHandler.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await searchPublicBusinessesHandler.HandleAsync(
            new SearchPublicBusinessesQuery(query, city, category, service, page, pageSize),
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(CreateInvalidSearchProblemDetails(result.Error));
        }

        return Ok(MapSearchResponse(result.Page!));
    }

    private ProblemDetails CreateInvalidSearchProblemDetails(PublicBusinessSearchError? error) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = error == PublicBusinessSearchError.InvalidPagination ? "Invalid pagination." : "Invalid search filter.",
        Detail = error == PublicBusinessSearchError.InvalidPagination
            ? $"Page must be at least 1 and pageSize must be between 1 and {PublicBusinessSearchQueryHandler.MaxPageSize}."
            : "Search filters exceed the allowed length.",
        Instance = HttpContext.Request.Path
    };

    private static PublicBusinessSearchResponse MapSearchResponse(PublicBusinessSearchPage page) => new(
        page.Items.Select(MapBusinessCard).ToList(),
        page.Page,
        page.PageSize,
        page.TotalCount,
        page.HasNextPage);

    private static PublicBusinessCardResponse MapBusinessCard(PublicBusinessCardDetails business) => new(
        business.Id,
        business.Slug,
        business.Name,
        business.Description,
        business.City,
        business.CountryCode,
        business.Category,
        business.TimeZoneId,
        business.CurrencyCode,
        business.FeaturedServices.Select(MapFeaturedService).ToList(),
        business.StartingPriceAmount);

    private static PublicBusinessFeaturedServiceResponse MapFeaturedService(PublicBusinessFeaturedServiceDetails service) => new(
        service.Id,
        service.Name,
        service.DurationMinutes,
        service.PriceAmount);
}
