using Calendar.Application.Businesses;
using Calendar.Domain.Entities;
using Calendar.Infrastructure.Persistence;
using Calendar.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Api.Tests.Businesses;

public sealed class PublicBusinessSearchRepositoryTests
{
    [Fact]
    public async Task SearchAsync_WithoutFilters_ReturnsActiveBusinessesOrderedByMostRecent()
    {
        using var dbContext = CreateDbContext();
        var olderBusiness = CreateBusiness(
            "barberia-centro",
            "Barberia Centro",
            "Barberia",
            "Madrid",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            [CreateServiceSeed("Corte", 18m, isActive: true), CreateServiceSeed("Afeitado", 12m, isActive: true)]);
        var newerBusiness = CreateBusiness(
            "yoga-norte",
            "Yoga Norte",
            "Clases",
            "Madrid",
            new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
            [CreateServiceSeed("Clase suelta", 25m, isActive: true)]);
        var inactiveBusiness = CreateBusiness(
            "fisioterapia-inactiva",
            "Fisioterapia Inactiva",
            "Fisioterapia",
            "Valencia",
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            [CreateServiceSeed("Sesion", 40m, isActive: true)],
            isActive: false);
        dbContext.Businesses.AddRange(olderBusiness, newerBusiness, inactiveBusiness);
        await dbContext.SaveChangesAsync();
        var repository = new PublicBusinessSearchRepository(dbContext);

        var result = await repository.SearchAsync(CreateCriteria(pageSize: 10), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Select(item => item.Slug).Should().Equal("yoga-norte", "barberia-centro");
        result.Items[0].FeaturedServices.Should().ContainSingle();
        result.Items[0].StartingPriceAmount.Should().Be(25m);
        result.HasNextPage.Should().BeFalse();
    }

    [Theory]
    [InlineData("Barberia Centro")]
    [InlineData("barrio")]
    [InlineData("Madrid")]
    [InlineData("Barberia")]
    [InlineData("Corte")]
    [InlineData("clasico")]
    public async Task SearchAsync_QueryFilter_MatchesBusinessAndActiveServiceFields(string query)
    {
        using var dbContext = CreateDbContext();
        var matchingBusiness = CreateBusiness(
            "barberia-centro",
            "Barberia Centro",
            "Barberia",
            "Madrid",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            [CreateServiceSeed("Corte", 18m, "Corte clasico", isActive: true)],
            description: "Barberia de barrio");
        var otherBusiness = CreateBusiness(
            "estetica-sur",
            "Estetica Sur",
            "Estetica",
            "Sevilla",
            new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
            [CreateServiceSeed("Manicura", 20m, isActive: true)]);
        dbContext.Businesses.AddRange(matchingBusiness, otherBusiness);
        await dbContext.SaveChangesAsync();
        var repository = new PublicBusinessSearchRepository(dbContext);

        var result = await repository.SearchAsync(CreateCriteria(query: query), CancellationToken.None);

        result.Items.Should().ContainSingle(item => item.Slug == "barberia-centro");
    }

    [Fact]
    public async Task SearchAsync_CityCategoryAndServiceFilters_AreCombinable()
    {
        using var dbContext = CreateDbContext();
        dbContext.Businesses.AddRange(
            CreateBusiness(
                "barberia-madrid",
                "Barberia Madrid",
                "Barberia",
                "Madrid",
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                [CreateServiceSeed("Corte", 18m, isActive: true)]),
            CreateBusiness(
                "barberia-valencia",
                "Barberia Valencia",
                "Barberia",
                "Valencia",
                new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
                [CreateServiceSeed("Corte", 18m, isActive: true)]),
            CreateBusiness(
                "estetica-madrid",
                "Estetica Madrid",
                "Estetica",
                "Madrid",
                new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero),
                [CreateServiceSeed("Corte", 18m, isActive: true)]));
        await dbContext.SaveChangesAsync();
        var repository = new PublicBusinessSearchRepository(dbContext);

        var result = await repository.SearchAsync(
            CreateCriteria(city: "Madrid", category: "Barberia", service: "Corte"),
            CancellationToken.None);

        result.Items.Should().ContainSingle(item => item.Slug == "barberia-madrid");
    }

    [Fact]
    public async Task SearchAsync_ServiceFilter_UsesOnlyActiveServicesAndCardsIncludeOnlyActiveServices()
    {
        using var dbContext = CreateDbContext();
        dbContext.Businesses.AddRange(
            CreateBusiness(
                "inactive-service-only",
                "Inactive Service Only",
                "Estetica",
                "Madrid",
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                [CreateServiceSeed("Masaje", 10m, isActive: false)]),
            CreateBusiness(
                "active-service",
                "Active Service",
                "Estetica",
                "Madrid",
                new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
                [CreateServiceSeed("Masaje", 35m, isActive: true), CreateServiceSeed("Servicio oculto", 5m, isActive: false)]));
        await dbContext.SaveChangesAsync();
        var repository = new PublicBusinessSearchRepository(dbContext);

        var result = await repository.SearchAsync(CreateCriteria(service: "Masaje"), CancellationToken.None);

        result.Items.Should().ContainSingle(item => item.Slug == "active-service");
        result.Items[0].FeaturedServices.Should().ContainSingle(service => service.Name == "Masaje");
        result.Items[0].StartingPriceAmount.Should().Be(35m);
    }

    [Fact]
    public async Task SearchAsync_AppliesPagination()
    {
        using var dbContext = CreateDbContext();
        dbContext.Businesses.AddRange(
            CreateBusiness("business-1", "Business 1", "Consultas", "Madrid", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateBusiness("business-2", "Business 2", "Consultas", "Madrid", new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero)),
            CreateBusiness("business-3", "Business 3", "Consultas", "Madrid", new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero)));
        await dbContext.SaveChangesAsync();
        var repository = new PublicBusinessSearchRepository(dbContext);

        var result = await repository.SearchAsync(CreateCriteria(page: 2, pageSize: 1), CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().Be(3);
        result.Items.Should().ContainSingle(item => item.Slug == "business-2");
        result.HasNextPage.Should().BeTrue();
    }

    private static CalendarDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CalendarDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CalendarDbContext(options);
    }

    private static PublicBusinessSearchCriteria CreateCriteria(
        string? query = null,
        string? city = null,
        string? category = null,
        string? service = null,
        int page = 1,
        int pageSize = 12) => new(query, city, category, service, page, pageSize);

    private static Business CreateBusiness(
        string slug,
        string name,
        string category,
        string city,
        DateTimeOffset createdAtUtc,
        IReadOnlyList<ServiceSeed>? services = null,
        bool isActive = true,
        string? description = null)
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Category = category,
            Description = description,
            City = city,
            CountryCode = "ES",
            TimeZoneId = "Europe/Madrid",
            CurrencyCode = "EUR",
            IsActive = isActive,
            CreatedAtUtc = createdAtUtc
        };

        foreach (var seed in services ?? Array.Empty<ServiceSeed>())
        {
            business.Services.Add(new Service
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                Business = business,
                Name = seed.Name,
                Description = seed.Description,
                DurationMinutes = 30,
                PriceAmount = seed.PriceAmount,
                IsActive = seed.IsActive,
                SortOrder = seed.SortOrder,
                CreatedAtUtc = createdAtUtc
            });
        }

        return business;
    }

    private static ServiceSeed CreateServiceSeed(
        string name,
        decimal priceAmount,
        string? description = null,
        bool isActive = true,
        int sortOrder = 0) => new(name, description, priceAmount, isActive, sortOrder);

    private sealed record ServiceSeed(
        string Name,
        string? Description,
        decimal PriceAmount,
        bool IsActive,
        int SortOrder);
}
