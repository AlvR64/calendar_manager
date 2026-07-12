using System.Security.Claims;
using Calendar.Api.Contracts.StaffMemberServices;
using Calendar.Api.Contracts.StaffMembers;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberServices.AssignStaffMemberService;
using Calendar.Application.StaffMembers;
using Calendar.Application.StaffMembers.CreateStaffMember;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class StaffMembersControllerTests
{
    [Fact]
    public async Task CreateStaffMember_WhenRequestIsValid_ReturnsCreatedResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCreateStaffMemberHandler(CreateStaffMemberResult.Success(
            staffMemberId,
            businessId,
            "Laura Martinez",
            "laura@example.test",
            "+34600999888",
            "Especialista en cortes y color",
            true,
            0,
            createdAtUtc));
        var controller = CreateController(handler, businessId.ToString());

        var result = await controller.CreateStaffMember(CreateRequest(), CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/api/staff-members/{staffMemberId}");

        var response = createdResult.Value.Should().BeOfType<StaffMemberResponse>().Subject;
        response.Id.Should().Be(staffMemberId);
        response.BusinessId.Should().Be(businessId);
        response.DisplayName.Should().Be("Laura Martinez");
        response.Email.Should().Be("laura@example.test");
        response.PhoneNumber.Should().Be("+34600999888");
        response.Bio.Should().Be("Especialista en cortes y color");
        response.IsActive.Should().BeTrue();
        response.SortOrder.Should().Be(0);
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task CreateStaffMember_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubCreateStaffMemberHandler(CreateStaffMemberResult.Success(
            Guid.NewGuid(),
            businessId,
            "Laura Martinez",
            "laura@example.test",
            "+34600999888",
            "Especialista en cortes y color",
            true,
            0,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessId.ToString());
        var request = CreateRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.CreateStaffMember(request, cancellationTokenSource.Token);

        handler.Command.Should().NotBeNull();
        handler.Command!.BusinessId.Should().Be(businessId);
        handler.Command.DisplayName.Should().Be(request.DisplayName);
        handler.Command.Email.Should().Be(request.Email);
        handler.Command.PhoneNumber.Should().Be(request.PhoneNumber);
        handler.Command.Bio.Should().Be(request.Bio);
        handler.Command.SortOrder.Should().Be(request.SortOrder);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task CreateStaffMember_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var handler = new StubCreateStaffMemberHandler(CreateStaffMemberResult.Success(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Laura Martinez",
            null,
            null,
            null,
            true,
            0,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessIdClaimValue: null);

        var result = await controller.CreateStaffMember(CreateRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task CreateStaffMember_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var handler = new StubCreateStaffMemberHandler(CreateStaffMemberResult.Success(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Laura Martinez",
            null,
            null,
            null,
            true,
            0,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, "not-a-guid");

        var result = await controller.CreateStaffMember(CreateRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task CreateStaffMember_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubCreateStaffMemberHandler(
            CreateStaffMemberResult.Failure(CreateStaffMemberError.BusinessNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString());

        var result = await controller.CreateStaffMember(CreateRequest(), CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be("/api/staff-members");
    }

    [Fact]
    public async Task ListStaffMembers_WhenBusinessExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult>(
            ListAdminStaffMembersResult.Success([
                new AdminStaffMemberDetails(
                    staffMemberId,
                    businessId,
                    "Laura Martinez",
                    "laura@example.test",
                    "+34600999888",
                    "Especialista en cortes y color",
                    false,
                    0,
                    createdAtUtc)
            ]));
        var controller = CreateController(handler, businessId.ToString(), "/api/staff-members");

        var result = await controller.ListStaffMembers(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<StaffMemberResponse>>().Subject;
        response.Should().ContainSingle();
        response[0].Id.Should().Be(staffMemberId);
        response[0].BusinessId.Should().Be(businessId);
        response[0].DisplayName.Should().Be("Laura Martinez");
        response[0].Email.Should().Be("laura@example.test");
        response[0].PhoneNumber.Should().Be("+34600999888");
        response[0].Bio.Should().Be("Especialista en cortes y color");
        response[0].IsActive.Should().BeFalse();
        response[0].SortOrder.Should().Be(0);
        response[0].CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task ListStaffMembers_WhenBusinessHasNoStaffMembers_ReturnsOkResponseWithEmptyList()
    {
        var handler = new StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult>(
            ListAdminStaffMembersResult.Success([]));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), "/api/staff-members");

        var result = await controller.ListStaffMembers(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<StaffMemberResponse>>().Subject;
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task ListStaffMembers_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult>(
            ListAdminStaffMembersResult.Success([]));
        var controller = CreateController(handler, businessId.ToString(), "/api/staff-members");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.ListStaffMembers(cancellationTokenSource.Token);

        handler.Query.Should().Be(new ListAdminStaffMembersQuery(businessId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task ListStaffMembers_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var handler = new StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult>(
            ListAdminStaffMembersResult.Success([]));
        var controller = CreateController(handler, businessIdClaimValue: null, "/api/staff-members");

        var result = await controller.ListStaffMembers(CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Query.Should().BeNull();
    }

    [Fact]
    public async Task ListStaffMembers_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var handler = new StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult>(
            ListAdminStaffMembersResult.Success([]));
        var controller = CreateController(handler, "not-a-guid", "/api/staff-members");

        var result = await controller.ListStaffMembers(CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Query.Should().BeNull();
    }

    [Fact]
    public async Task ListStaffMembers_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult>(
            ListAdminStaffMembersResult.NotFound());
        var controller = CreateController(handler, Guid.NewGuid().ToString(), "/api/staff-members");

        var result = await controller.ListStaffMembers(CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be("/api/staff-members");
    }

    [Fact]
    public async Task GetStaffMember_WhenStaffMemberExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult>(
            GetAdminStaffMemberResult.Success(new AdminStaffMemberDetails(
                staffMemberId,
                businessId,
                "Laura Martinez",
                "laura@example.test",
                "+34600999888",
                "Especialista en cortes y color",
                false,
                0,
                createdAtUtc)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}");

        var result = await controller.GetStaffMember(staffMemberId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StaffMemberResponse>().Subject;
        response.Id.Should().Be(staffMemberId);
        response.BusinessId.Should().Be(businessId);
        response.DisplayName.Should().Be("Laura Martinez");
        response.IsActive.Should().BeFalse();
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task GetStaffMember_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult>(
            GetAdminStaffMemberResult.Success(CreateAdminStaffMemberDetails(businessId, staffMemberId)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.GetStaffMember(staffMemberId, cancellationTokenSource.Token);

        handler.Query.Should().Be(new GetAdminStaffMemberQuery(businessId, staffMemberId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task GetStaffMember_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult>(
            GetAdminStaffMemberResult.Success(CreateAdminStaffMemberDetails(Guid.NewGuid(), staffMemberId)));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}");

        var result = await controller.GetStaffMember(staffMemberId, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Query.Should().BeNull();
    }

    [Fact]
    public async Task GetStaffMember_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult>(
            GetAdminStaffMemberResult.Success(CreateAdminStaffMemberDetails(Guid.NewGuid(), staffMemberId)));
        var controller = CreateController(handler, "not-a-guid", $"/api/staff-members/{staffMemberId}");

        var result = await controller.GetStaffMember(staffMemberId, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Query.Should().BeNull();
    }

    [Fact]
    public async Task GetStaffMember_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult>(
            GetAdminStaffMemberResult.Failure(GetAdminStaffMemberError.BusinessNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}");

        var result = await controller.GetStaffMember(staffMemberId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}");
    }

    [Fact]
    public async Task GetStaffMember_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult>(
            GetAdminStaffMemberResult.Failure(GetAdminStaffMemberError.StaffMemberNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}");

        var result = await controller.GetStaffMember(staffMemberId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}");
    }

    [Fact]
    public async Task AssignServiceToStaffMember_WhenAssignmentSucceeds_ReturnsCreatedResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult.Success(
            staffMemberId,
            serviceId,
            true,
            createdAtUtc));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.AssignServiceToStaffMember(staffMemberId, serviceId, CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = objectResult.Value.Should().BeOfType<StaffMemberServiceAssignmentResponse>().Subject;
        response.StaffMemberId.Should().Be(staffMemberId);
        response.ServiceId.Should().Be(serviceId);
        response.IsActive.Should().BeTrue();
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task AssignServiceToStaffMember_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult.Success(
            staffMemberId,
            serviceId,
            true,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.AssignServiceToStaffMember(staffMemberId, serviceId, cancellationTokenSource.Token);

        handler.Command.Should().NotBeNull();
        handler.Command!.BusinessId.Should().Be(businessId);
        handler.Command.StaffMemberId.Should().Be(staffMemberId);
        handler.Command.ServiceId.Should().Be(serviceId);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task AssignServiceToStaffMember_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult.Success(
            staffMemberId,
            serviceId,
            true,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.AssignServiceToStaffMember(staffMemberId, serviceId, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task AssignServiceToStaffMember_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(AssignStaffMemberServiceResult.Success(
            staffMemberId,
            serviceId,
            true,
            DateTimeOffset.UtcNow));
        var controller = CreateController(handler, "not-a-guid", $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.AssignServiceToStaffMember(staffMemberId, serviceId, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task AssignServiceToStaffMember_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(
            AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.StaffMemberNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.AssignServiceToStaffMember(staffMemberId, serviceId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/services/{serviceId}");
    }

    [Fact]
    public async Task AssignServiceToStaffMember_WhenServiceDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(
            AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.ServiceNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.AssignServiceToStaffMember(staffMemberId, serviceId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Service not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/services/{serviceId}");
    }

    [Fact]
    public async Task AssignServiceToStaffMember_WhenAssignmentAlreadyExists_ReturnsConflictProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubAssignStaffMemberServiceHandler(
            AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.AssignmentAlreadyExists));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.AssignServiceToStaffMember(staffMemberId, serviceId, CancellationToken.None);

        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problemDetails = conflictResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Staff member service assignment already exists.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/services/{serviceId}");
    }

    private static StaffMembersController CreateController(
        StubCreateStaffMemberHandler handler,
        string? businessIdClaimValue) =>
        CreateController(
            handler,
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            "/api/staff-members");

    private static StaffMembersController CreateController(
        StubAssignStaffMemberServiceHandler handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            handler,
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            handler,
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            handler,
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCreateStaffMemberHandler createStaffMemberHandler,
        StubAssignStaffMemberServiceHandler assignStaffMemberServiceHandler,
        StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult> listAdminStaffMembersHandler,
        StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult> getAdminStaffMemberHandler,
        string? businessIdClaimValue,
        string requestPath)
    {
        var claims = new List<Claim>();
        if (businessIdClaimValue is not null)
        {
            claims.Add(new Claim("business_id", businessIdClaimValue));
        }

        return new StaffMembersController(
            createStaffMemberHandler,
            assignStaffMemberServiceHandler,
            listAdminStaffMembersHandler,
            getAdminStaffMemberHandler)
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

    private static CreateStaffMemberRequest CreateRequest() => new()
    {
        DisplayName = "Laura Martinez",
        Email = "laura@example.test",
        PhoneNumber = "+34600999888",
        Bio = "Especialista en cortes y color",
        SortOrder = 0
    };

    private static AdminStaffMemberDetails CreateAdminStaffMemberDetails(Guid businessId, Guid staffMemberId) => new(
        staffMemberId,
        businessId,
        "Laura Martinez",
        "laura@example.test",
        "+34600999888",
        "Especialista en cortes y color",
        true,
        0,
        DateTimeOffset.UtcNow);

    private static StubCreateStaffMemberHandler CreateDefaultCreateStaffMemberHandler() => new(CreateStaffMemberResult.Success(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Laura Martinez",
        "laura@example.test",
        "+34600999888",
        "Especialista en cortes y color",
        true,
        0,
        DateTimeOffset.UtcNow));

    private static StubAssignStaffMemberServiceHandler CreateDefaultAssignStaffMemberServiceHandler() => new(AssignStaffMemberServiceResult.Success(
        Guid.NewGuid(),
        Guid.NewGuid(),
        true,
        DateTimeOffset.UtcNow));

    private static StubQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult> CreateDefaultListAdminStaffMembersHandler() => new(
        ListAdminStaffMembersResult.Success([]));

    private static StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult> CreateDefaultGetAdminStaffMemberHandler()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        return new StubQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult>(
            GetAdminStaffMemberResult.Success(CreateAdminStaffMemberDetails(businessId, staffMemberId)));
    }

    private sealed class StubCreateStaffMemberHandler(CreateStaffMemberResult result)
        : ICommandHandler<CreateStaffMemberCommand, CreateStaffMemberResult>
    {
        public CreateStaffMemberCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<CreateStaffMemberResult> HandleAsync(
            CreateStaffMemberCommand command,
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
