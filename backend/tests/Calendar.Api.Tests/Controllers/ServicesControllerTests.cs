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

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/api/services/{serviceId}");

        var response = createdResult.Value.Should().BeOfType<ServiceResponse>().Subject;
        response.Id.Should().Be(serviceId);
        response.BusinessId.Should().Be(businessId);
        response.Name.Should().Be("Corte de pelo");
        response.Description.Should().Be("Corte clasico o moderno");
        response.DurationMinutes.Should().Be(30);
        response.PriceAmount.Should().Be(18.00m);
        response.IsActive.Should().BeTrue();
        response.SortOrder.Should().Be(0);
        response.CreatedAtUtc.Should().Be(createdAtUtc);
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

        handler.Command.Should().NotBeNull();
        handler.Command!.BusinessId.Should().Be(businessId);
        handler.Command.Name.Should().Be(request.Name);
        handler.Command.Description.Should().Be(request.Description);
        handler.Command.DurationMinutes.Should().Be(request.DurationMinutes);
        handler.Command.PriceAmount.Should().Be(request.PriceAmount);
        handler.Command.SortOrder.Should().Be(request.SortOrder);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
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

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
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

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task CreateService_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubCreateServiceHandler(
            CreateServiceResult.Failure(CreateServiceError.BusinessNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString());

        var result = await controller.CreateService(CreateRequest(), CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be("/api/services");
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
