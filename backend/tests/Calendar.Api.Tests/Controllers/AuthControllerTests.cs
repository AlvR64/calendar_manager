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

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginAdminResponse>().Subject;
        response.AccessToken.Should().Be("admin-token");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresAtUtc.Should().Be(expiresAtUtc);
        response.User.Type.Should().Be("Admin");
        response.User.Id.Should().Be(adminId);
        response.User.BusinessId.Should().Be(businessId);
        response.User.Email.Should().Be("admin@barberia-centro.test");
        response.User.DisplayName.Should().Be("Admin Centro");
    }

    [Fact]
    public async Task LoginAdmin_WhenCredentialsAreInvalid_ReturnsUnauthorizedProblemDetails()
    {
        var handler = new StubLoginAdminHandler(LoginAdminResult.Failure(LoginAdminError.InvalidCredentials));
        var controller = CreateController(handler);

        var result = await controller.LoginAdmin(CreateLoginAdminRequest(), CancellationToken.None);

        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var problemDetails = unauthorizedResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails.Title.Should().Be("Invalid credentials.");
        problemDetails.Instance.Should().Be("/api/auth/admin/login");
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

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginCustomerResponse>().Subject;
        response.AccessToken.Should().Be("customer-token");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresAtUtc.Should().Be(expiresAtUtc);
        response.User.Type.Should().Be("Customer");
        response.User.Id.Should().Be(customerId);
        response.User.Email.Should().Be("customer@example.test");
        response.User.FirstName.Should().Be("Carlos");
        response.User.LastName.Should().Be("Garcia");
    }

    [Fact]
    public async Task LoginCustomer_WhenAccountIsInactive_ReturnsForbiddenProblemDetails()
    {
        var handler = new StubLoginCustomerHandler(LoginCustomerResult.Failure(LoginCustomerError.AccountInactive));
        var controller = CreateController(handler);

        var result = await controller.LoginCustomer(CreateLoginCustomerRequest(), CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemDetails.Title.Should().Be("Account inactive.");
        problemDetails.Instance.Should().Be("/api/auth/customer/login");
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

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/api/businesses/{businessId}");

        var response = createdResult.Value.Should().BeOfType<RegisterBusinessResponse>().Subject;
        response.BusinessId.Should().Be(businessId);
        response.BusinessSlug.Should().Be("barberia-centro");
        response.AdminId.Should().Be(adminId);
        response.AdminEmail.Should().Be("admin@barberia-centro.test");
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task RegisterBusiness_WhenBusinessSlugAlreadyExists_ReturnsConflictProblemDetails()
    {
        var handler = new StubRegisterBusinessHandler(
            RegisterBusinessResult.Failure(RegisterBusinessError.BusinessSlugAlreadyExists));
        var controller = CreateController(handler);

        var result = await controller.RegisterBusiness(CreateRegisterBusinessRequest(), CancellationToken.None);

        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problemDetails = conflictResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Business slug already exists.");
        problemDetails.Instance.Should().Be("/api/auth/register-business");
    }

    [Fact]
    public async Task RegisterBusiness_WhenTimeZoneIdIsInvalid_ReturnsBadRequestProblemDetails()
    {
        var handler = new StubRegisterBusinessHandler(
            RegisterBusinessResult.Failure(RegisterBusinessError.InvalidTimeZoneId));
        var controller = CreateController(handler);

        var result = await controller.RegisterBusiness(CreateRegisterBusinessRequest(), CancellationToken.None);

        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Invalid time zone.");
        problemDetails.Instance.Should().Be("/api/auth/register-business");
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

        handler.Command.Should().NotBeNull();
        handler.Command!.BusinessName.Should().Be(request.BusinessName);
        handler.Command.BusinessSlug.Should().Be(request.BusinessSlug);
        handler.Command.TimeZoneId.Should().Be(request.TimeZoneId);
        handler.Command.CurrencyCode.Should().Be(request.CurrencyCode);
        handler.Command.AdminEmail.Should().Be(request.AdminEmail);
        handler.Command.AdminPassword.Should().Be(request.AdminPassword);
        handler.Command.AdminDisplayName.Should().Be(request.AdminDisplayName);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
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

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/api/customers/{customerId}");

        var response = createdResult.Value.Should().BeOfType<RegisterCustomerResponse>().Subject;
        response.CustomerId.Should().Be(customerId);
        response.Email.Should().Be("customer@example.test");
        response.FirstName.Should().Be("Carlos");
        response.LastName.Should().Be("Garcia");
        response.PhoneNumber.Should().Be("+34600111222");
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task RegisterCustomer_WhenCustomerEmailAlreadyExists_ReturnsConflictProblemDetails()
    {
        var handler = new StubRegisterCustomerHandler(
            RegisterCustomerResult.Failure(RegisterCustomerError.CustomerEmailAlreadyExists));
        var controller = CreateController(handler);

        var result = await controller.RegisterCustomer(CreateRegisterCustomerRequest(), CancellationToken.None);

        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problemDetails = conflictResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Customer email already exists.");
        problemDetails.Instance.Should().Be("/api/auth/register-customer");
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

        handler.Command.Should().NotBeNull();
        handler.Command!.Email.Should().Be(request.Email);
        handler.Command.Password.Should().Be(request.Password);
        handler.Command.FirstName.Should().Be(request.FirstName);
        handler.Command.LastName.Should().Be(request.LastName);
        handler.Command.PhoneNumber.Should().Be(request.PhoneNumber);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
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
