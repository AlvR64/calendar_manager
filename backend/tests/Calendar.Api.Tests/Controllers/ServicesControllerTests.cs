using System.Security.Claims;
using Calendar.Api.Contracts.Services;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Services.CreateService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class ServicesControllerTests
{
    [Fact]
    public async Task CreateService_WhenRequestIsValid_ReturnsCreatedResponse()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCreateServiceHandler(CreateServiceResult.Success(
            serviceId,
            businessId,
            "Corte de pelo",
            "Corte clasico o moderno",
            30,
            18.00m,
            true,
            0,
            createdAtUtc));
        var controller = CreateController(handler, businessId.ToString());

        var result = await controller.CreateService(CreateRequest(), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal($"/api/services/{serviceId}", createdResult.Location);

        var response = Assert.IsType<ServiceResponse>(createdResult.Value);
        Assert.Equal(serviceId, response.Id);
        Assert.Equal(businessId, response.BusinessId);
        Assert.Equal("Corte de pelo", response.Name);
        Assert.Equal("Corte clasico o moderno", response.Description);
        Assert.Equal(30, response.DurationMinutes);
        Assert.Equal(18.00m, response.PriceAmount);
        Assert.True(response.IsActive);
        Assert.Equal(0, response.SortOrder);
        Assert.Equal(createdAtUtc, response.CreatedAtUtc);
    }

    [Fact]
    public async Task CreateService_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubCreateServiceHandler(CreateServiceResult.Success(
            Guid.NewGuid(),
            businessId,
            "Corte de pelo",
            "Corte clasico o moderno",
            30,
            18.00m,
            true,
            0,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessId.ToString());
        var request = CreateRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.CreateService(request, cancellationTokenSource.Token);

        Assert.NotNull(handler.Command);
        Assert.Equal(businessId, handler.Command.BusinessId);
        Assert.Equal(request.Name, handler.Command.Name);
        Assert.Equal(request.Description, handler.Command.Description);
        Assert.Equal(request.DurationMinutes, handler.Command.DurationMinutes);
        Assert.Equal(request.PriceAmount, handler.Command.PriceAmount);
        Assert.Equal(request.SortOrder, handler.Command.SortOrder);
        Assert.Equal(cancellationTokenSource.Token, handler.CancellationToken);
    }

    [Fact]
    public async Task CreateService_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var handler = new StubCreateServiceHandler(CreateServiceResult.Success(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Corte de pelo",
            null,
            30,
            18.00m,
            true,
            0,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessIdClaimValue: null);

        var result = await controller.CreateService(CreateRequest(), CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Null(handler.Command);
    }

    [Fact]
    public async Task CreateService_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var handler = new StubCreateServiceHandler(CreateServiceResult.Success(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Corte de pelo",
            null,
            30,
            18.00m,
            true,
            0,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, "not-a-guid");

        var result = await controller.CreateService(CreateRequest(), CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Null(handler.Command);
    }

    [Fact]
    public async Task CreateService_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubCreateServiceHandler(
            CreateServiceResult.Failure(CreateServiceError.BusinessNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString());

        var result = await controller.CreateService(CreateRequest(), CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("Business not found.", problemDetails.Title);
        Assert.Equal("/api/services", problemDetails.Instance);
    }

    private static ServicesController CreateController(
        StubCreateServiceHandler handler,
        string? businessIdClaimValue)
    {
        var claims = new List<Claim>();
        if (businessIdClaimValue is not null)
        {
            claims.Add(new Claim("business_id", businessIdClaimValue));
        }

        return new ServicesController(handler)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")),
                    Request =
                    {
                        Path = "/api/services"
                    }
                }
            }
        };
    }

    private static CreateServiceRequest CreateRequest() => new()
    {
        Name = "Corte de pelo",
        Description = "Corte clasico o moderno",
        DurationMinutes = 30,
        PriceAmount = 18.00m,
        SortOrder = 0
    };

    private sealed class StubCreateServiceHandler(CreateServiceResult result)
        : ICommandHandler<CreateServiceCommand, CreateServiceResult>
    {
        public CreateServiceCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<CreateServiceResult> HandleAsync(
            CreateServiceCommand command,
            CancellationToken cancellationToken)
        {
            Command = command;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
