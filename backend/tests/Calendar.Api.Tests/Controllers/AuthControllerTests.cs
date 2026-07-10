using Calendar.Api.Contracts.Auth;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.RegisterBusiness;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task RegisterBusiness_WhenRegistrationSucceeds_ReturnsCreatedResponse()
    {
        var businessId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubRegisterBusinessHandler(
            RegisterBusinessResult.Success(
                businessId,
                "barberia-centro",
                adminId,
                "admin@barberia-centro.test",
                createdAtUtc));
        var controller = CreateController(handler);

        var result = await controller.RegisterBusiness(CreateRequest(), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal($"/api/businesses/{businessId}", createdResult.Location);

        var response = Assert.IsType<RegisterBusinessResponse>(createdResult.Value);
        Assert.Equal(businessId, response.BusinessId);
        Assert.Equal("barberia-centro", response.BusinessSlug);
        Assert.Equal(adminId, response.AdminId);
        Assert.Equal("admin@barberia-centro.test", response.AdminEmail);
        Assert.Equal(createdAtUtc, response.CreatedAtUtc);
    }

    [Fact]
    public async Task RegisterBusiness_WhenBusinessSlugAlreadyExists_ReturnsConflictProblemDetails()
    {
        var handler = new StubRegisterBusinessHandler(
            RegisterBusinessResult.Failure(RegisterBusinessError.BusinessSlugAlreadyExists));
        var controller = CreateController(handler);

        var result = await controller.RegisterBusiness(CreateRequest(), CancellationToken.None);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("Business slug already exists.", problemDetails.Title);
        Assert.Equal("/api/auth/register-business", problemDetails.Instance);
    }

    [Fact]
    public async Task RegisterBusiness_MapsRequestToCommand()
    {
        var handler = new StubRegisterBusinessHandler(RegisterBusinessResult.Success(
            Guid.NewGuid(),
            "barberia-centro",
            Guid.NewGuid(),
            "admin@barberia-centro.test",
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler);
        var request = CreateRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.RegisterBusiness(request, cancellationTokenSource.Token);

        Assert.NotNull(handler.Command);
        Assert.Equal(request.BusinessName, handler.Command.BusinessName);
        Assert.Equal(request.BusinessSlug, handler.Command.BusinessSlug);
        Assert.Equal(request.TimeZoneId, handler.Command.TimeZoneId);
        Assert.Equal(request.CurrencyCode, handler.Command.CurrencyCode);
        Assert.Equal(request.AdminEmail, handler.Command.AdminEmail);
        Assert.Equal(request.AdminPassword, handler.Command.AdminPassword);
        Assert.Equal(request.AdminDisplayName, handler.Command.AdminDisplayName);
        Assert.Equal(cancellationTokenSource.Token, handler.CancellationToken);
    }

    private static AuthController CreateController(StubRegisterBusinessHandler handler) => new(handler)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Path = "/api/auth/register-business"
                }
            }
        }
    };

    private static RegisterBusinessRequest CreateRequest() => new()
    {
        BusinessName = "Barberia Centro",
        BusinessSlug = "barberia-centro",
        TimeZoneId = "Europe/Madrid",
        CurrencyCode = "EUR",
        AdminEmail = "admin@barberia-centro.test",
        AdminPassword = "ChangeMe123!",
        AdminDisplayName = "Admin Centro"
    };

    private sealed class StubRegisterBusinessHandler(RegisterBusinessResult result)
        : ICommandHandler<RegisterBusinessCommand, RegisterBusinessResult>
    {
        public RegisterBusinessCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<RegisterBusinessResult> HandleAsync(
            RegisterBusinessCommand command,
            CancellationToken cancellationToken)
        {
            Command = command;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
