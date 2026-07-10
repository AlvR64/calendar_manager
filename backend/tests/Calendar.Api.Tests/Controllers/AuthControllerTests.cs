using Calendar.Api.Contracts.Auth;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.RegisterBusiness;
using Calendar.Application.Auth.RegisterCustomer;
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

    [Fact]
    public async Task RegisterCustomer_WhenRegistrationSucceeds_ReturnsCreatedResponse()
    {
        var customerId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubRegisterCustomerHandler(
            RegisterCustomerResult.Success(
                customerId,
                "customer@example.test",
                "Carlos",
                "Garcia",
                "+34600111222",
                createdAtUtc));
        var controller = CreateController(handler);

        var result = await controller.RegisterCustomer(CreateCustomerRequest(), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal($"/api/customers/{customerId}", createdResult.Location);

        var response = Assert.IsType<RegisterCustomerResponse>(createdResult.Value);
        Assert.Equal(customerId, response.CustomerId);
        Assert.Equal("customer@example.test", response.Email);
        Assert.Equal("Carlos", response.FirstName);
        Assert.Equal("Garcia", response.LastName);
        Assert.Equal("+34600111222", response.PhoneNumber);
        Assert.Equal(createdAtUtc, response.CreatedAtUtc);
    }

    [Fact]
    public async Task RegisterCustomer_WhenCustomerEmailAlreadyExists_ReturnsConflictProblemDetails()
    {
        var handler = new StubRegisterCustomerHandler(
            RegisterCustomerResult.Failure(RegisterCustomerError.CustomerEmailAlreadyExists));
        var controller = CreateController(handler);

        var result = await controller.RegisterCustomer(CreateCustomerRequest(), CancellationToken.None);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("Customer email already exists.", problemDetails.Title);
        Assert.Equal("/api/auth/register-customer", problemDetails.Instance);
    }

    [Fact]
    public async Task RegisterCustomer_MapsRequestToCommand()
    {
        var handler = new StubRegisterCustomerHandler(RegisterCustomerResult.Success(
            Guid.NewGuid(),
            "customer@example.test",
            "Carlos",
            "Garcia",
            "+34600111222",
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler);
        var request = CreateCustomerRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.RegisterCustomer(request, cancellationTokenSource.Token);

        Assert.NotNull(handler.Command);
        Assert.Equal(request.Email, handler.Command.Email);
        Assert.Equal(request.Password, handler.Command.Password);
        Assert.Equal(request.FirstName, handler.Command.FirstName);
        Assert.Equal(request.LastName, handler.Command.LastName);
        Assert.Equal(request.PhoneNumber, handler.Command.PhoneNumber);
        Assert.Equal(cancellationTokenSource.Token, handler.CancellationToken);
    }

    private static AuthController CreateController(StubRegisterBusinessHandler handler) =>
        CreateController(handler, CreateDefaultCustomerHandler(), "/api/auth/register-business");

    private static AuthController CreateController(StubRegisterCustomerHandler handler) =>
        CreateController(CreateDefaultBusinessHandler(), handler, "/api/auth/register-customer");

    private static AuthController CreateController(
        StubRegisterBusinessHandler businessHandler,
        StubRegisterCustomerHandler customerHandler,
        string requestPath) => new(businessHandler, customerHandler)
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

    private static RegisterCustomerRequest CreateCustomerRequest() => new()
    {
        Email = "customer@example.test",
        Password = "ChangeMe123!",
        FirstName = "Carlos",
        LastName = "Garcia",
        PhoneNumber = "+34600111222"
    };

    private static StubRegisterBusinessHandler CreateDefaultBusinessHandler() => new(RegisterBusinessResult.Success(
        Guid.NewGuid(),
        "barberia-centro",
        Guid.NewGuid(),
        "admin@barberia-centro.test",
        DateTimeOffset.UtcNow));

    private static StubRegisterCustomerHandler CreateDefaultCustomerHandler() => new(RegisterCustomerResult.Success(
        Guid.NewGuid(),
        "customer@example.test",
        "Carlos",
        "Garcia",
        "+34600111222",
        DateTimeOffset.UtcNow));

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

    private sealed class StubRegisterCustomerHandler(RegisterCustomerResult result)
        : ICommandHandler<RegisterCustomerCommand, RegisterCustomerResult>
    {
        public RegisterCustomerCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<RegisterCustomerResult> HandleAsync(
            RegisterCustomerCommand command,
            CancellationToken cancellationToken)
        {
            Command = command;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
