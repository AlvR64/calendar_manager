using Calendar.Api.Contracts.Auth;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.LoginAdmin;
using Calendar.Application.Auth.LoginCustomer;
using Calendar.Application.Auth.RegisterBusiness;
using Calendar.Application.Auth.RegisterCustomer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task LoginAdmin_WhenCredentialsAreValid_ReturnsOkResponse()
    {
        var adminId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(60);
        var handler = new StubLoginAdminHandler(LoginAdminResult.Success(
            "admin-token",
            "Bearer",
            expiresAtUtc,
            adminId,
            businessId,
            "admin@barberia-centro.test",
            "Admin Centro"));
        var controller = CreateController(handler);

        var result = await controller.LoginAdmin(CreateLoginAdminRequest(), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginAdminResponse>(okResult.Value);
        Assert.Equal("admin-token", response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(expiresAtUtc, response.ExpiresAtUtc);
        Assert.Equal("Admin", response.User.Type);
        Assert.Equal(adminId, response.User.Id);
        Assert.Equal(businessId, response.User.BusinessId);
        Assert.Equal("admin@barberia-centro.test", response.User.Email);
        Assert.Equal("Admin Centro", response.User.DisplayName);
    }

    [Fact]
    public async Task LoginAdmin_WhenCredentialsAreInvalid_ReturnsUnauthorizedProblemDetails()
    {
        var handler = new StubLoginAdminHandler(LoginAdminResult.Failure(LoginAdminError.InvalidCredentials));
        var controller = CreateController(handler);

        var result = await controller.LoginAdmin(CreateLoginAdminRequest(), CancellationToken.None);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
        Assert.Equal("Invalid credentials.", problemDetails.Title);
        Assert.Equal("/api/auth/admin/login", problemDetails.Instance);
    }

    [Fact]
    public async Task LoginCustomer_WhenCredentialsAreValid_ReturnsOkResponse()
    {
        var customerId = Guid.NewGuid();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(60);
        var handler = new StubLoginCustomerHandler(LoginCustomerResult.Success(
            "customer-token",
            "Bearer",
            expiresAtUtc,
            customerId,
            "customer@example.test",
            "Carlos",
            "Garcia"));
        var controller = CreateController(handler);

        var result = await controller.LoginCustomer(CreateLoginCustomerRequest(), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginCustomerResponse>(okResult.Value);
        Assert.Equal("customer-token", response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(expiresAtUtc, response.ExpiresAtUtc);
        Assert.Equal("Customer", response.User.Type);
        Assert.Equal(customerId, response.User.Id);
        Assert.Equal("customer@example.test", response.User.Email);
        Assert.Equal("Carlos", response.User.FirstName);
        Assert.Equal("Garcia", response.User.LastName);
    }

    [Fact]
    public async Task LoginCustomer_WhenAccountIsInactive_ReturnsForbiddenProblemDetails()
    {
        var handler = new StubLoginCustomerHandler(LoginCustomerResult.Failure(LoginCustomerError.AccountInactive));
        var controller = CreateController(handler);

        var result = await controller.LoginCustomer(CreateLoginCustomerRequest(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("Account inactive.", problemDetails.Title);
        Assert.Equal("/api/auth/customer/login", problemDetails.Instance);
    }

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

        var result = await controller.RegisterBusiness(CreateRegisterBusinessRequest(), CancellationToken.None);

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

        var result = await controller.RegisterBusiness(CreateRegisterBusinessRequest(), CancellationToken.None);

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
        var request = CreateRegisterBusinessRequest();
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

        var result = await controller.RegisterCustomer(CreateRegisterCustomerRequest(), CancellationToken.None);

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

        var result = await controller.RegisterCustomer(CreateRegisterCustomerRequest(), CancellationToken.None);

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
        var request = CreateRegisterCustomerRequest();
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

    private static AuthController CreateController(StubLoginAdminHandler handler) =>
        CreateController(handler, CreateDefaultLoginCustomerHandler(), CreateDefaultRegisterBusinessHandler(), CreateDefaultRegisterCustomerHandler(), "/api/auth/admin/login");

    private static AuthController CreateController(StubLoginCustomerHandler handler) =>
        CreateController(CreateDefaultLoginAdminHandler(), handler, CreateDefaultRegisterBusinessHandler(), CreateDefaultRegisterCustomerHandler(), "/api/auth/customer/login");

    private static AuthController CreateController(StubRegisterBusinessHandler handler) =>
        CreateController(CreateDefaultLoginAdminHandler(), CreateDefaultLoginCustomerHandler(), handler, CreateDefaultRegisterCustomerHandler(), "/api/auth/register-business");

    private static AuthController CreateController(StubRegisterCustomerHandler handler) =>
        CreateController(CreateDefaultLoginAdminHandler(), CreateDefaultLoginCustomerHandler(), CreateDefaultRegisterBusinessHandler(), handler, "/api/auth/register-customer");

    private static AuthController CreateController(
        StubLoginAdminHandler loginAdminHandler,
        StubLoginCustomerHandler loginCustomerHandler,
        StubRegisterBusinessHandler registerBusinessHandler,
        StubRegisterCustomerHandler registerCustomerHandler,
        string requestPath) => new(loginAdminHandler, loginCustomerHandler, registerBusinessHandler, registerCustomerHandler)
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

    private static LoginAdminRequest CreateLoginAdminRequest() => new()
    {
        Email = "admin@barberia-centro.test",
        Password = "ChangeMe123!"
    };

    private static LoginCustomerRequest CreateLoginCustomerRequest() => new()
    {
        Email = "customer@example.test",
        Password = "ChangeMe123!"
    };

    private static RegisterBusinessRequest CreateRegisterBusinessRequest() => new()
    {
        BusinessName = "Barberia Centro",
        BusinessSlug = "barberia-centro",
        TimeZoneId = "Europe/Madrid",
        CurrencyCode = "EUR",
        AdminEmail = "admin@barberia-centro.test",
        AdminPassword = "ChangeMe123!",
        AdminDisplayName = "Admin Centro"
    };

    private static RegisterCustomerRequest CreateRegisterCustomerRequest() => new()
    {
        Email = "customer@example.test",
        Password = "ChangeMe123!",
        FirstName = "Carlos",
        LastName = "Garcia",
        PhoneNumber = "+34600111222"
    };

    private static StubLoginAdminHandler CreateDefaultLoginAdminHandler() => new(LoginAdminResult.Success(
        "admin-token",
        "Bearer",
        DateTimeOffset.UtcNow.AddMinutes(60),
        Guid.NewGuid(),
        Guid.NewGuid(),
        "admin@barberia-centro.test",
        "Admin Centro"));

    private static StubLoginCustomerHandler CreateDefaultLoginCustomerHandler() => new(LoginCustomerResult.Success(
        "customer-token",
        "Bearer",
        DateTimeOffset.UtcNow.AddMinutes(60),
        Guid.NewGuid(),
        "customer@example.test",
        "Carlos",
        "Garcia"));

    private static StubRegisterBusinessHandler CreateDefaultRegisterBusinessHandler() => new(RegisterBusinessResult.Success(
        Guid.NewGuid(),
        "barberia-centro",
        Guid.NewGuid(),
        "admin@barberia-centro.test",
        DateTimeOffset.UtcNow));

    private static StubRegisterCustomerHandler CreateDefaultRegisterCustomerHandler() => new(RegisterCustomerResult.Success(
        Guid.NewGuid(),
        "customer@example.test",
        "Carlos",
        "Garcia",
        "+34600111222",
        DateTimeOffset.UtcNow));

    private sealed class StubLoginAdminHandler(LoginAdminResult result)
        : ICommandHandler<LoginAdminCommand, LoginAdminResult>
    {
        public Task<LoginAdminResult> HandleAsync(LoginAdminCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(result);
    }

    private sealed class StubLoginCustomerHandler(LoginCustomerResult result)
        : ICommandHandler<LoginCustomerCommand, LoginCustomerResult>
    {
        public Task<LoginCustomerResult> HandleAsync(LoginCustomerCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(result);
    }

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
