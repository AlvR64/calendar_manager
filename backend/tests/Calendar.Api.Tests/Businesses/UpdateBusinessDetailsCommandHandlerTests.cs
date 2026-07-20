using Calendar.Application.Businesses.UpdateBusinessDetails;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Api.Tests.Businesses;

public sealed class UpdateBusinessDetailsCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NormalizesLegacyCategoryLabelsBeforeSaving()
    {
        var businessId = Guid.NewGuid();
        var business = new Business
        {
            Id = businessId,
            Name = "Clinica Centro",
            Slug = "clinica-centro",
            TimeZoneId = "Europe/Madrid",
            CurrencyCode = "EUR"
        };
        var handler = new UpdateBusinessDetailsCommandHandler(
            new FakeBusinessRepository(business),
            new ValidTimeZoneProvider(),
            new FakeUnitOfWork());

        var result = await handler.HandleAsync(new UpdateBusinessDetailsCommand(
            businessId,
            "Clinica Centro",
            " Fisioterapia ",
            null,
            null,
            null,
            null,
            null,
            null,
            "Valencia",
            null,
            "ES",
            "Europe/Madrid",
            "EUR"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Category.Should().Be("physiotherapy");
        business.Category.Should().Be("physiotherapy");
    }

    private sealed class FakeBusinessRepository(Business? business) : IBusinessRepository
    {
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(business?.Id == id);

        public Task<bool> ExistsActiveByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(business is { IsActive: true } && business.Id == id);

        public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken) => Task.FromResult(business?.Slug == slug);

        public Task<Business?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(business is { IsActive: true } && business.Id == id ? business : null);

        public Task<Business?> GetActiveBySlugAsync(string slug, CancellationToken cancellationToken) => Task.FromResult(business is { IsActive: true } && business.Slug == slug ? business : null);

        public Task<Business?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(business?.Id == id ? business : null);

        public void Add(Business businessToAdd)
        {
        }
    }

    private sealed class ValidTimeZoneProvider : ITimeZoneProvider
    {
        public bool TryGetIanaTimeZoneInfo(string timeZoneId, out TimeZoneInfo timeZoneInfo)
        {
            timeZoneInfo = TimeZoneInfo.Utc;
            return true;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
