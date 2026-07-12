using System.Security.Claims;
using Calendar.Api.Contracts.StaffMemberServices;
using Calendar.Api.Contracts.Services;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Services;
using Calendar.Application.Services.CreateService;
using Calendar.Application.StaffMemberServices.AssignStaffMemberService;
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

    [Fact]
    public async Task ListServices_WhenBusinessExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult>(
            ListAdminServicesResult.Success([
                new AdminServiceDetails(
                    serviceId,
                    businessId,
                    "Corte de pelo",
                    "Corte clasico o moderno",
                    30,
                    18.00m,
                    false,
                    0,
                    createdAtUtc)
            ]));
        var controller = CreateController(handler, businessId.ToString(), "/api/services");

        var result = await controller.ListServices(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<ServiceResponse>>().Subject;
        response.Should().ContainSingle();
        response[0].Id.Should().Be(serviceId);
        response[0].BusinessId.Should().Be(businessId);
        response[0].Name.Should().Be("Corte de pelo");
        response[0].IsActive.Should().BeFalse();
        response[0].CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task ListServices_WhenBusinessHasNoServices_ReturnsOkResponseWithEmptyList()
    {
        var handler = new StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult>(
            ListAdminServicesResult.Success([]));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), "/api/services");

        var result = await controller.ListServices(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<ServiceResponse>>().Subject;
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task ListServices_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult>(
            ListAdminServicesResult.Success([]));
        var controller = CreateController(handler, businessId.ToString(), "/api/services");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.ListServices(cancellationTokenSource.Token);

        handler.Query.Should().Be(new ListAdminServicesQuery(businessId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task ListServices_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var handler = new StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult>(
            ListAdminServicesResult.Success([]));
        var controller = CreateController(handler, businessIdClaimValue: null, "/api/services");

        var result = await controller.ListServices(CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Query.Should().BeNull();
    }

    [Fact]
    public async Task ListServices_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var handler = new StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult>(
            ListAdminServicesResult.Success([]));
        var controller = CreateController(handler, "not-a-guid", "/api/services");

        var result = await controller.ListServices(CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Query.Should().BeNull();
    }

    [Fact]
    public async Task ListServices_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult>(
            ListAdminServicesResult.NotFound());
        var controller = CreateController(handler, Guid.NewGuid().ToString(), "/api/services");

        var result = await controller.ListServices(CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be("/api/services");
    }

    [Fact]
    public async Task GetService_WhenServiceExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>(
            GetAdminServiceResult.Success(new AdminServiceDetails(
                serviceId,
                businessId,
                "Corte de pelo",
                "Corte clasico o moderno",
                30,
                18.00m,
                false,
                0,
                createdAtUtc)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/services/{serviceId}");

        var result = await controller.GetService(serviceId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ServiceResponse>().Subject;
        response.Id.Should().Be(serviceId);
        response.BusinessId.Should().Be(businessId);
        response.Name.Should().Be("Corte de pelo");
        response.IsActive.Should().BeFalse();
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task GetService_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>(
            GetAdminServiceResult.Success(CreateAdminServiceDetails(businessId, serviceId)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/services/{serviceId}");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.GetService(serviceId, cancellationTokenSource.Token);

        handler.Query.Should().Be(new GetAdminServiceQuery(businessId, serviceId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task GetService_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var serviceId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>(
            GetAdminServiceResult.Success(CreateAdminServiceDetails(Guid.NewGuid(), serviceId)));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/services/{serviceId}");

        var result = await controller.GetService(serviceId, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Query.Should().BeNull();
    }

    [Fact]
    public async Task GetService_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var serviceId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>(
            GetAdminServiceResult.Success(CreateAdminServiceDetails(Guid.NewGuid(), serviceId)));
        var controller = CreateController(handler, "not-a-guid", $"/api/services/{serviceId}");

        var result = await controller.GetService(serviceId, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Query.Should().BeNull();
    }

    [Fact]
    public async Task GetService_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var serviceId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>(
            GetAdminServiceResult.Failure(GetAdminServiceError.BusinessNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/services/{serviceId}");

        var result = await controller.GetService(serviceId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be($"/api/services/{serviceId}");
    }

    [Fact]
    public async Task GetService_WhenServiceDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var serviceId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>(
            GetAdminServiceResult.Failure(GetAdminServiceError.ServiceNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/services/{serviceId}");

        var result = await controller.GetService(serviceId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Service not found.");
        problemDetails.Instance.Should().Be($"/api/services/{serviceId}");
    }

    [Fact]
    public async Task AssignStaffMemberToService_WhenAssignmentSucceeds_ReturnsCreatedResponse()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult.Success(
            staffMemberId,
            serviceId,
            true,
            createdAtUtc));
        var controller = CreateController(handler, businessId.ToString(), $"/api/services/{serviceId}/staff-members/{staffMemberId}");

        var result = await controller.AssignStaffMemberToService(serviceId, staffMemberId, CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeOfType<StaffMemberServiceAssignmentResponse>().Subject;
        response.StaffMemberId.Should().Be(staffMemberId);
        response.ServiceId.Should().Be(serviceId);
        response.IsActive.Should().BeTrue();
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task AssignStaffMemberToService_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult.Success(
            staffMemberId,
            serviceId,
            true,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessId.ToString(), $"/api/services/{serviceId}/staff-members/{staffMemberId}");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.AssignStaffMemberToService(serviceId, staffMemberId, cancellationTokenSource.Token);

        handler.Command.Should().NotBeNull();
        handler.Command!.BusinessId.Should().Be(businessId);
        handler.Command.StaffMemberId.Should().Be(staffMemberId);
        handler.Command.ServiceId.Should().Be(serviceId);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task AssignStaffMemberToService_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult.Success(
            staffMemberId,
            serviceId,
            true,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/services/{serviceId}/staff-members/{staffMemberId}");

        var result = await controller.AssignStaffMemberToService(serviceId, staffMemberId, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task AssignStaffMemberToService_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult.Success(
            staffMemberId,
            serviceId,
            true,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, "not-a-guid", $"/api/services/{serviceId}/staff-members/{staffMemberId}");

        var result = await controller.AssignStaffMemberToService(serviceId, staffMemberId, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task AssignStaffMemberToService_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(
            AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.StaffMemberNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/services/{serviceId}/staff-members/{staffMemberId}");

        var result = await controller.AssignStaffMemberToService(serviceId, staffMemberId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/services/{serviceId}/staff-members/{staffMemberId}");
    }

    [Fact]
    public async Task AssignStaffMemberToService_WhenServiceDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(
            AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.ServiceNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/services/{serviceId}/staff-members/{staffMemberId}");

        var result = await controller.AssignStaffMemberToService(serviceId, staffMemberId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Service not found.");
        problemDetails.Instance.Should().Be($"/api/services/{serviceId}/staff-members/{staffMemberId}");
    }

    [Fact]
    public async Task AssignStaffMemberToService_WhenAssignmentAlreadyExists_ReturnsConflictProblemDetails()
    {
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(
            AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.AssignmentAlreadyExists));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/services/{serviceId}/staff-members/{staffMemberId}");

        var result = await controller.AssignStaffMemberToService(serviceId, staffMemberId, CancellationToken.None);

        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problemDetails = conflictResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Staff member service assignment already exists.");
        problemDetails.Instance.Should().Be($"/api/services/{serviceId}/staff-members/{staffMemberId}");
    }

    private static ServicesController CreateController(
        StubCreateServiceHandler handler,
        string? businessIdClaimValue) =>
        CreateController(
            handler,
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultListAdminServicesHandler(),
            CreateDefaultGetAdminServiceHandler(),
            businessIdClaimValue,
            "/api/services");

    private static ServicesController CreateController(
        StubAssignStaffMemberServiceHandler handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateServiceHandler(),
            handler,
            CreateDefaultListAdminServicesHandler(),
            CreateDefaultGetAdminServiceHandler(),
            businessIdClaimValue,
            requestPath);

    private static ServicesController CreateController(
        StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateServiceHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            handler,
            CreateDefaultGetAdminServiceHandler(),
            businessIdClaimValue,
            requestPath);

    private static ServicesController CreateController(
        StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateServiceHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultListAdminServicesHandler(),
            handler,
            businessIdClaimValue,
            requestPath);

    private static ServicesController CreateController(
        StubCreateServiceHandler createServiceHandler,
        StubAssignStaffMemberServiceHandler assignStaffMemberServiceHandler,
        StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult> listAdminServicesHandler,
        StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult> getAdminServiceHandler,
        string? businessIdClaimValue,
        string requestPath)
    {
        var claims = new List<Claim>();
        if (businessIdClaimValue is not null)
        {
            claims.Add(new Claim("business_id", businessIdClaimValue));
        }

        return new ServicesController(
            createServiceHandler,
            assignStaffMemberServiceHandler,
            listAdminServicesHandler,
            getAdminServiceHandler)
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

    private static CreateServiceRequest CreateRequest() => new()
    {
        Name = "Corte de pelo",
        Description = "Corte clasico o moderno",
        DurationMinutes = 30,
        PriceAmount = 18.00m,
        SortOrder = 0
    };

    private static AdminServiceDetails CreateAdminServiceDetails(Guid businessId, Guid serviceId) => new(
        serviceId,
        businessId,
        "Corte de pelo",
        "Corte clasico o moderno",
        30,
        18.00m,
        true,
        0,
        DateTimeOffset.UtcNow);

    private static StubCreateServiceHandler CreateDefaultCreateServiceHandler() => new(CreateServiceResult.Success(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Corte de pelo",
        "Corte clasico o moderno",
        30,
        18.00m,
        true,
        0,
        DateTimeOffset.UtcNow));

    private static StubAssignStaffMemberServiceHandler CreateDefaultAssignStaffMemberServiceHandler() => new(AssignStaffMemberServiceResult.Success(
        Guid.NewGuid(),
        Guid.NewGuid(),
        true,
        DateTimeOffset.UtcNow));

    private static StubQueryHandler<ListAdminServicesQuery, ListAdminServicesResult> CreateDefaultListAdminServicesHandler() => new(
        ListAdminServicesResult.Success([]));

    private static StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult> CreateDefaultGetAdminServiceHandler()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        return new StubQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>(
            GetAdminServiceResult.Success(CreateAdminServiceDetails(businessId, serviceId)));
    }

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

    private sealed class StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult result)
        : ICommandHandler<AssignStaffMemberServiceCommand, AssignStaffMemberServiceResult>
    {
        public AssignStaffMemberServiceCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<AssignStaffMemberServiceResult> HandleAsync(
            AssignStaffMemberServiceCommand command,
            CancellationToken cancellationToken)
        {
            Command = command;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }

    private sealed class StubQueryHandler<TQuery, TResult>(TResult result) : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        public TQuery? Query { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            Query = query;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
