using System.Security.Claims;
using Calendar.Api.Contracts.Businesses;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Businesses.UpdateBusinessBookingWindow;
using Calendar.Application.Businesses.UpdateBusinessDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class CurrentBusinessControllerTests
{
    [Fact]
    public async Task UpdateBusinessDetails_WhenRequestIsValid_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubUpdateBusinessDetailsHandler(UpdateBusinessDetailsResult.Success(
            businessId,
            "Barberia Centro",
            "barberia-centro-madrid",
            "Barberia de barrio",
            "contacto@barberia.test",
            "+34910000000",
            "https://barberia.test",
            "Calle Mayor 1",
            null,
            "Madrid",
            "28013",
            "ES",
            "Europe/Madrid",
            "EUR",
            60));
        var controller = CreateController(handler, businessId.ToString(), "/api/businesses/current");

        var result = await controller.UpdateBusinessDetails(CreateUpdateBusinessDetailsRequest(), CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BusinessResponse>().Subject;
        response.Id.Should().Be(businessId);
        response.Name.Should().Be("Barberia Centro");
        response.Slug.Should().Be("barberia-centro-madrid");
        response.CountryCode.Should().Be("ES");
        response.CurrencyCode.Should().Be("EUR");
        response.MaxAdvanceBookingDays.Should().Be(60);
    }

    [Fact]
    public async Task UpdateBusinessDetails_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubUpdateBusinessDetailsHandler(CreateSuccessfulUpdateBusinessDetailsResult(businessId));
        var controller = CreateController(handler, businessId.ToString(), "/api/businesses/current");
        var request = CreateUpdateBusinessDetailsRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.UpdateBusinessDetails(request, cancellationTokenSource.Token);

        handler.Command.Should().NotBeNull();
        handler.Command!.BusinessId.Should().Be(businessId);
        handler.Command.Name.Should().Be(request.Name);
        handler.Command.CountryCode.Should().Be(request.CountryCode);
        handler.Command.CurrencyCode.Should().Be(request.CurrencyCode);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task UpdateBusinessDetails_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var handler = new StubUpdateBusinessDetailsHandler(CreateSuccessfulUpdateBusinessDetailsResult(Guid.NewGuid()));
        var controller = CreateController(handler, businessIdClaimValue: null, "/api/businesses/current");

        var result = await controller.UpdateBusinessDetails(CreateUpdateBusinessDetailsRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBusinessDetails_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var handler = new StubUpdateBusinessDetailsHandler(CreateSuccessfulUpdateBusinessDetailsResult(Guid.NewGuid()));
        var controller = CreateController(handler, "not-a-guid", "/api/businesses/current");

        var result = await controller.UpdateBusinessDetails(CreateUpdateBusinessDetailsRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBusinessDetails_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubUpdateBusinessDetailsHandler(
            UpdateBusinessDetailsResult.Failure(UpdateBusinessDetailsError.BusinessNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), "/api/businesses/current");

        var result = await controller.UpdateBusinessDetails(CreateUpdateBusinessDetailsRequest(), CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be("/api/businesses/current");
    }

    [Fact]
    public async Task UpdateBusinessBookingWindow_WhenRequestIsValid_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubUpdateBusinessBookingWindowHandler(UpdateBusinessBookingWindowResult.Success(90));
        var controller = CreateController(handler, businessId.ToString(), "/api/businesses/current/booking-window");

        var result = await controller.UpdateBusinessBookingWindow(new UpdateBusinessBookingWindowRequest
        {
            MaxAdvanceBookingDays = 90
        }, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BusinessBookingWindowResponse>().Subject;
        response.MaxAdvanceBookingDays.Should().Be(90);
    }

    [Fact]
    public async Task UpdateBusinessBookingWindow_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubUpdateBusinessBookingWindowHandler(UpdateBusinessBookingWindowResult.Success(120));
        var controller = CreateController(handler, businessId.ToString(), "/api/businesses/current/booking-window");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.UpdateBusinessBookingWindow(new UpdateBusinessBookingWindowRequest
        {
            MaxAdvanceBookingDays = 120
        }, cancellationTokenSource.Token);

        handler.Command.Should().NotBeNull();
        handler.Command!.BusinessId.Should().Be(businessId);
        handler.Command.MaxAdvanceBookingDays.Should().Be(120);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task UpdateBusinessBookingWindow_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var handler = new StubUpdateBusinessBookingWindowHandler(UpdateBusinessBookingWindowResult.Success(60));
        var controller = CreateController(handler, businessIdClaimValue: null, "/api/businesses/current/booking-window");

        var result = await controller.UpdateBusinessBookingWindow(new UpdateBusinessBookingWindowRequest
        {
            MaxAdvanceBookingDays = 60
        }, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBusinessBookingWindow_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubUpdateBusinessBookingWindowHandler(
            UpdateBusinessBookingWindowResult.Failure(UpdateBusinessBookingWindowError.BusinessNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), "/api/businesses/current/booking-window");

        var result = await controller.UpdateBusinessBookingWindow(new UpdateBusinessBookingWindowRequest
        {
            MaxAdvanceBookingDays = 60
        }, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be("/api/businesses/current/booking-window");
    }

    private static CurrentBusinessController CreateController(
        StubUpdateBusinessDetailsHandler handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(handler, CreateDefaultUpdateBusinessBookingWindowHandler(), businessIdClaimValue, requestPath);

    private static CurrentBusinessController CreateController(
        StubUpdateBusinessBookingWindowHandler handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(CreateDefaultUpdateBusinessDetailsHandler(), handler, businessIdClaimValue, requestPath);

    private static CurrentBusinessController CreateController(
        StubUpdateBusinessDetailsHandler updateBusinessDetailsHandler,
        StubUpdateBusinessBookingWindowHandler updateBusinessBookingWindowHandler,
        string? businessIdClaimValue,
        string requestPath)
    {
        var claims = new List<Claim>();
        if (businessIdClaimValue is not null)
        {
            claims.Add(new Claim("business_id", businessIdClaimValue));
        }

        return new CurrentBusinessController(updateBusinessDetailsHandler, updateBusinessBookingWindowHandler)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")),
                    Request =
                    {
                        Path = requestPath
                    }
                }
            }
        };
    }

    private static UpdateBusinessDetailsRequest CreateUpdateBusinessDetailsRequest() => new()
    {
        Name = "Barberia Centro",
        Description = "Barberia de barrio",
        ContactEmail = "contacto@barberia.test",
        ContactPhoneNumber = "+34910000000",
        WebsiteUrl = "https://barberia.test",
        AddressLine1 = "Calle Mayor 1",
        AddressLine2 = null,
        City = "Madrid",
        PostalCode = "28013",
        CountryCode = "es",
        TimeZoneId = "Europe/Madrid",
        CurrencyCode = "eur"
    };

    private static UpdateBusinessDetailsResult CreateSuccessfulUpdateBusinessDetailsResult(Guid businessId) =>
        UpdateBusinessDetailsResult.Success(
            businessId,
            "Barberia Centro",
            "barberia-centro-madrid",
            "Barberia de barrio",
            "contacto@barberia.test",
            "+34910000000",
            "https://barberia.test",
            "Calle Mayor 1",
            null,
            "Madrid",
            "28013",
            "ES",
            "Europe/Madrid",
            "EUR",
            60);

    private static StubUpdateBusinessDetailsHandler CreateDefaultUpdateBusinessDetailsHandler() => new(
        CreateSuccessfulUpdateBusinessDetailsResult(Guid.NewGuid()));

    private static StubUpdateBusinessBookingWindowHandler CreateDefaultUpdateBusinessBookingWindowHandler() => new(
        UpdateBusinessBookingWindowResult.Success(60));

    private sealed class StubUpdateBusinessDetailsHandler(UpdateBusinessDetailsResult result)
        : ICommandHandler<UpdateBusinessDetailsCommand, UpdateBusinessDetailsResult>
    {
        public UpdateBusinessDetailsCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<UpdateBusinessDetailsResult> HandleAsync(
            UpdateBusinessDetailsCommand command,
            CancellationToken cancellationToken)
        {
            Command = command;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }

    private sealed class StubUpdateBusinessBookingWindowHandler(UpdateBusinessBookingWindowResult result)
        : ICommandHandler<UpdateBusinessBookingWindowCommand, UpdateBusinessBookingWindowResult>
    {
        public UpdateBusinessBookingWindowCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<UpdateBusinessBookingWindowResult> HandleAsync(
            UpdateBusinessBookingWindowCommand command,
            CancellationToken cancellationToken)
        {
            Command = command;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
