using System.Security.Claims;
using Calendar.Api.Contracts.StaffMemberServices;
using Calendar.Api.Contracts.StaffMembers;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilities;
using Calendar.Application.StaffMemberAvailabilities.CreateStaffMemberAvailability;
using Calendar.Application.StaffMemberAvailabilities.DeleteStaffMemberAvailability;
using Calendar.Application.StaffMemberAvailabilities.ListStaffMemberAvailabilities;
using Calendar.Application.StaffMemberAvailabilities.UpdateStaffMemberAvailability;
using Calendar.Application.StaffMemberAvailabilityExceptions;
using Calendar.Application.StaffMemberAvailabilityExceptions.CreateStaffMemberAvailabilityException;
using Calendar.Application.StaffMemberAvailabilityExceptions.DeleteStaffMemberAvailabilityException;
using Calendar.Application.StaffMemberAvailabilityExceptions.ListStaffMemberAvailabilityExceptions;
using Calendar.Application.StaffMemberAvailabilityExceptions.UpdateStaffMemberAvailabilityException;
using Calendar.Application.StaffMemberServices.AssignStaffMemberService;
using Calendar.Application.StaffMemberServices.UnassignStaffMemberService;
using Calendar.Application.StaffMemberServices.UpdateStaffMemberServiceActiveState;
using Calendar.Application.StaffMembers;
using Calendar.Application.StaffMembers.CreateStaffMember;
using Calendar.Application.StaffMembers.DeleteStaffMember;
using Calendar.Application.StaffMembers.UpdateStaffMember;
using Calendar.Application.StaffMembers.UpdateStaffMemberActiveState;
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
    public async Task CreateStaffMemberAvailability_WhenRequestIsValid_ReturnsCreatedResponse()
    {
        var staffMemberId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult>(
            CreateStaffMemberAvailabilityResult.Success(CreateAvailabilityDetails(availabilityId, staffMemberId, createdAtUtc)));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability");

        var result = await controller.CreateStaffMemberAvailability(staffMemberId, CreateAvailabilityRequest(), CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/api/staff-members/{staffMemberId}/availability/{availabilityId}");
        var response = createdResult.Value.Should().BeOfType<StaffMemberAvailabilityResponse>().Subject;
        response.Id.Should().Be(availabilityId);
        response.StaffMemberId.Should().Be(staffMemberId);
        response.DayOfWeek.Should().Be(1);
        response.StartTime.Should().Be(new TimeOnly(9, 0));
        response.EndTime.Should().Be(new TimeOnly(13, 0));
        response.IsActive.Should().BeTrue();
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task CreateStaffMemberAvailability_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult>(
            CreateStaffMemberAvailabilityResult.Success(CreateAvailabilityDetails(Guid.NewGuid(), staffMemberId, DateTimeOffset.UtcNow)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/availability");
        var request = CreateAvailabilityRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.CreateStaffMemberAvailability(staffMemberId, request, cancellationTokenSource.Token);

        handler.Command.Should().Be(new CreateStaffMemberAvailabilityCommand(
            businessId,
            staffMemberId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task CreateStaffMemberAvailability_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult>(
            CreateStaffMemberAvailabilityResult.Success(CreateAvailabilityDetails(Guid.NewGuid(), staffMemberId, DateTimeOffset.UtcNow)));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}/availability");

        var result = await controller.CreateStaffMemberAvailability(staffMemberId, CreateAvailabilityRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Theory]
    [InlineData(CreateStaffMemberAvailabilityError.StaffMemberNotFound, typeof(NotFoundObjectResult), "Staff member not found.")]
    [InlineData(CreateStaffMemberAvailabilityError.InvalidDayOfWeek, typeof(BadRequestObjectResult), "Invalid availability day of week.")]
    [InlineData(CreateStaffMemberAvailabilityError.InvalidTimeRange, typeof(BadRequestObjectResult), "Invalid availability time range.")]
    [InlineData(CreateStaffMemberAvailabilityError.AvailabilityOverlaps, typeof(ConflictObjectResult), "Staff member availability overlaps.")]
    public async Task CreateStaffMemberAvailability_WhenHandlerFails_ReturnsProblemDetails(
        CreateStaffMemberAvailabilityError error,
        Type expectedResultType,
        string expectedTitle)
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult>(
            CreateStaffMemberAvailabilityResult.Failure(error));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability");

        var result = await controller.CreateStaffMemberAvailability(staffMemberId, CreateAvailabilityRequest(), CancellationToken.None);

        result.Result.Should().BeOfType(expectedResultType);
        var objectResult = result.Result.Should().BeAssignableTo<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/availability");
    }

    [Fact]
    public async Task ListStaffMemberAvailabilities_WhenStaffMemberExists_ReturnsOkResponse()
    {
        var staffMemberId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubQueryHandler<ListStaffMemberAvailabilitiesQuery, ListStaffMemberAvailabilitiesResult>(
            ListStaffMemberAvailabilitiesResult.Success([CreateAvailabilityDetails(availabilityId, staffMemberId, createdAtUtc)]));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability");

        var result = await controller.ListStaffMemberAvailabilities(staffMemberId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<StaffMemberAvailabilityResponse>>().Subject;
        response.Should().ContainSingle();
        response[0].Id.Should().Be(availabilityId);
        response[0].StaffMemberId.Should().Be(staffMemberId);
        response[0].CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task ListStaffMemberAvailabilities_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<ListStaffMemberAvailabilitiesQuery, ListStaffMemberAvailabilitiesResult>(
            ListStaffMemberAvailabilitiesResult.Success([]));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/availability");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.ListStaffMemberAvailabilities(staffMemberId, cancellationTokenSource.Token);

        handler.Query.Should().Be(new ListStaffMemberAvailabilitiesQuery(businessId, staffMemberId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task ListStaffMemberAvailabilities_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<ListStaffMemberAvailabilitiesQuery, ListStaffMemberAvailabilitiesResult>(
            ListStaffMemberAvailabilitiesResult.Failure(ListStaffMemberAvailabilitiesError.StaffMemberNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability");

        var result = await controller.ListStaffMemberAvailabilities(staffMemberId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/availability");
    }

    [Fact]
    public async Task CreateStaffMemberAvailabilityException_WhenRequestIsValid_ReturnsCreatedResponse()
    {
        var staffMemberId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult>(
            CreateStaffMemberAvailabilityExceptionResult.Success(CreateAvailabilityExceptionDetails(exceptionId, staffMemberId, createdAtUtc)));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions");

        var result = await controller.CreateStaffMemberAvailabilityException(staffMemberId, CreateAvailabilityExceptionRequest(), CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");
        var response = createdResult.Value.Should().BeOfType<StaffMemberAvailabilityExceptionResponse>().Subject;
        response.Id.Should().Be(exceptionId);
        response.StaffMemberId.Should().Be(staffMemberId);
        response.LocalDate.Should().Be(new DateOnly(2026, 7, 20));
        response.IsClosed.Should().BeFalse();
        response.StartTime.Should().Be(new TimeOnly(10, 0));
        response.EndTime.Should().Be(new TimeOnly(14, 0));
        response.Reason.Should().Be("Horario especial");
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task CreateStaffMemberAvailabilityException_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult>(
            CreateStaffMemberAvailabilityExceptionResult.Success(CreateAvailabilityExceptionDetails(Guid.NewGuid(), staffMemberId, DateTimeOffset.UtcNow)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions");
        var request = CreateAvailabilityExceptionRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.CreateStaffMemberAvailabilityException(staffMemberId, request, cancellationTokenSource.Token);

        handler.Command.Should().Be(new CreateStaffMemberAvailabilityExceptionCommand(
            businessId,
            staffMemberId,
            request.LocalDate,
            request.IsClosed,
            request.StartTime,
            request.EndTime,
            request.Reason));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task CreateStaffMemberAvailabilityException_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult>(
            CreateStaffMemberAvailabilityExceptionResult.Success(CreateAvailabilityExceptionDetails(Guid.NewGuid(), staffMemberId, DateTimeOffset.UtcNow)));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}/availability-exceptions");

        var result = await controller.CreateStaffMemberAvailabilityException(staffMemberId, CreateAvailabilityExceptionRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Theory]
    [InlineData(CreateStaffMemberAvailabilityExceptionError.StaffMemberNotFound, typeof(NotFoundObjectResult), "Staff member not found.")]
    [InlineData(CreateStaffMemberAvailabilityExceptionError.InvalidClosedException, typeof(BadRequestObjectResult), "Invalid closed availability exception.")]
    [InlineData(CreateStaffMemberAvailabilityExceptionError.InvalidTimeRange, typeof(BadRequestObjectResult), "Invalid availability exception time range.")]
    [InlineData(CreateStaffMemberAvailabilityExceptionError.ReasonTooLong, typeof(BadRequestObjectResult), "Availability exception reason is too long.")]
    [InlineData(CreateStaffMemberAvailabilityExceptionError.AvailabilityExceptionAlreadyExists, typeof(ConflictObjectResult), "Staff member availability exception already exists for this date.")]
    [InlineData(CreateStaffMemberAvailabilityExceptionError.AvailabilityExceptionOverlaps, typeof(ConflictObjectResult), "Staff member availability exception overlaps.")]
    public async Task CreateStaffMemberAvailabilityException_WhenHandlerFails_ReturnsProblemDetails(
        CreateStaffMemberAvailabilityExceptionError error,
        Type expectedResultType,
        string expectedTitle)
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult>(
            CreateStaffMemberAvailabilityExceptionResult.Failure(error));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions");

        var result = await controller.CreateStaffMemberAvailabilityException(staffMemberId, CreateAvailabilityExceptionRequest(), CancellationToken.None);

        result.Result.Should().BeOfType(expectedResultType);
        var objectResult = result.Result.Should().BeAssignableTo<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/availability-exceptions");
    }

    [Fact]
    public async Task ListStaffMemberAvailabilityExceptions_WhenStaffMemberExists_ReturnsOkResponse()
    {
        var staffMemberId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubQueryHandler<ListStaffMemberAvailabilityExceptionsQuery, ListStaffMemberAvailabilityExceptionsResult>(
            ListStaffMemberAvailabilityExceptionsResult.Success([CreateAvailabilityExceptionDetails(exceptionId, staffMemberId, createdAtUtc)]));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions");

        var result = await controller.ListStaffMemberAvailabilityExceptions(staffMemberId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<StaffMemberAvailabilityExceptionResponse>>().Subject;
        response.Should().ContainSingle();
        response[0].Id.Should().Be(exceptionId);
        response[0].StaffMemberId.Should().Be(staffMemberId);
        response[0].CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task ListStaffMemberAvailabilityExceptions_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<ListStaffMemberAvailabilityExceptionsQuery, ListStaffMemberAvailabilityExceptionsResult>(
            ListStaffMemberAvailabilityExceptionsResult.Success([]));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.ListStaffMemberAvailabilityExceptions(staffMemberId, cancellationTokenSource.Token);

        handler.Query.Should().Be(new ListStaffMemberAvailabilityExceptionsQuery(businessId, staffMemberId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task ListStaffMemberAvailabilityExceptions_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<ListStaffMemberAvailabilityExceptionsQuery, ListStaffMemberAvailabilityExceptionsResult>(
            ListStaffMemberAvailabilityExceptionsResult.Failure(ListStaffMemberAvailabilityExceptionsError.StaffMemberNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions");

        var result = await controller.ListStaffMemberAvailabilityExceptions(staffMemberId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/availability-exceptions");
    }

    [Fact]
    public async Task UpdateStaffMemberAvailabilityException_WhenExceptionExists_ReturnsOkResponse()
    {
        var staffMemberId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCommandHandler<UpdateStaffMemberAvailabilityExceptionCommand, UpdateStaffMemberAvailabilityExceptionResult>(
            UpdateStaffMemberAvailabilityExceptionResult.Success(CreateAvailabilityExceptionDetails(exceptionId, staffMemberId, createdAtUtc)));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");

        var result = await controller.UpdateStaffMemberAvailabilityException(
            staffMemberId,
            exceptionId,
            CreateUpdateAvailabilityExceptionRequest(),
            CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StaffMemberAvailabilityExceptionResponse>().Subject;
        response.Id.Should().Be(exceptionId);
        response.StaffMemberId.Should().Be(staffMemberId);
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task UpdateStaffMemberAvailabilityException_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberAvailabilityExceptionCommand, UpdateStaffMemberAvailabilityExceptionResult>(
            UpdateStaffMemberAvailabilityExceptionResult.Success(CreateAvailabilityExceptionDetails(exceptionId, staffMemberId, DateTimeOffset.UtcNow)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");
        var request = CreateUpdateAvailabilityExceptionRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.UpdateStaffMemberAvailabilityException(staffMemberId, exceptionId, request, cancellationTokenSource.Token);

        handler.Command.Should().Be(new UpdateStaffMemberAvailabilityExceptionCommand(
            businessId,
            staffMemberId,
            exceptionId,
            request.LocalDate,
            request.IsClosed,
            request.StartTime,
            request.EndTime,
            request.Reason));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Theory]
    [InlineData(UpdateStaffMemberAvailabilityExceptionError.StaffMemberNotFound, typeof(NotFoundObjectResult), "Staff member not found.")]
    [InlineData(UpdateStaffMemberAvailabilityExceptionError.AvailabilityExceptionNotFound, typeof(NotFoundObjectResult), "Staff member availability exception not found.")]
    [InlineData(UpdateStaffMemberAvailabilityExceptionError.InvalidClosedException, typeof(BadRequestObjectResult), "Invalid closed availability exception.")]
    [InlineData(UpdateStaffMemberAvailabilityExceptionError.InvalidTimeRange, typeof(BadRequestObjectResult), "Invalid availability exception time range.")]
    [InlineData(UpdateStaffMemberAvailabilityExceptionError.ReasonTooLong, typeof(BadRequestObjectResult), "Availability exception reason is too long.")]
    [InlineData(UpdateStaffMemberAvailabilityExceptionError.AvailabilityExceptionAlreadyExists, typeof(ConflictObjectResult), "Staff member availability exception already exists for this date.")]
    [InlineData(UpdateStaffMemberAvailabilityExceptionError.AvailabilityExceptionOverlaps, typeof(ConflictObjectResult), "Staff member availability exception overlaps.")]
    public async Task UpdateStaffMemberAvailabilityException_WhenHandlerFails_ReturnsProblemDetails(
        UpdateStaffMemberAvailabilityExceptionError error,
        Type expectedResultType,
        string expectedTitle)
    {
        var staffMemberId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberAvailabilityExceptionCommand, UpdateStaffMemberAvailabilityExceptionResult>(
            UpdateStaffMemberAvailabilityExceptionResult.Failure(error));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");

        var result = await controller.UpdateStaffMemberAvailabilityException(
            staffMemberId,
            exceptionId,
            CreateUpdateAvailabilityExceptionRequest(),
            CancellationToken.None);

        result.Result.Should().BeOfType(expectedResultType);
        var objectResult = result.Result.Should().BeAssignableTo<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");
    }

    [Fact]
    public async Task DeleteStaffMemberAvailabilityException_WhenExceptionExists_ReturnsNoContent()
    {
        var staffMemberId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberAvailabilityExceptionCommand, DeleteStaffMemberAvailabilityExceptionResult>(
            DeleteStaffMemberAvailabilityExceptionResult.Success());
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");

        var result = await controller.DeleteStaffMemberAvailabilityException(staffMemberId, exceptionId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteStaffMemberAvailabilityException_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberAvailabilityExceptionCommand, DeleteStaffMemberAvailabilityExceptionResult>(
            DeleteStaffMemberAvailabilityExceptionResult.Success());
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.DeleteStaffMemberAvailabilityException(staffMemberId, exceptionId, cancellationTokenSource.Token);

        handler.Command.Should().Be(new DeleteStaffMemberAvailabilityExceptionCommand(businessId, staffMemberId, exceptionId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Theory]
    [InlineData(DeleteStaffMemberAvailabilityExceptionError.StaffMemberNotFound, "Staff member not found.")]
    [InlineData(DeleteStaffMemberAvailabilityExceptionError.AvailabilityExceptionNotFound, "Staff member availability exception not found.")]
    public async Task DeleteStaffMemberAvailabilityException_WhenHandlerFails_ReturnsNotFoundProblemDetails(
        DeleteStaffMemberAvailabilityExceptionError error,
        string expectedTitle)
    {
        var staffMemberId = Guid.NewGuid();
        var exceptionId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberAvailabilityExceptionCommand, DeleteStaffMemberAvailabilityExceptionResult>(
            DeleteStaffMemberAvailabilityExceptionResult.Failure(error));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");

        var result = await controller.DeleteStaffMemberAvailabilityException(staffMemberId, exceptionId, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/availability-exceptions/{exceptionId}");
    }

    [Fact]
    public async Task UpdateStaffMemberAvailability_WhenAvailabilityExists_ReturnsOkResponse()
    {
        var staffMemberId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult>(
            UpdateStaffMemberAvailabilityResult.Success(CreateAvailabilityDetails(availabilityId, staffMemberId, createdAtUtc)));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability/{availabilityId}");

        var result = await controller.UpdateStaffMemberAvailability(
            staffMemberId,
            availabilityId,
            CreateUpdateAvailabilityRequest(),
            CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StaffMemberAvailabilityResponse>().Subject;
        response.Id.Should().Be(availabilityId);
        response.StaffMemberId.Should().Be(staffMemberId);
    }

    [Fact]
    public async Task UpdateStaffMemberAvailability_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult>(
            UpdateStaffMemberAvailabilityResult.Success(CreateAvailabilityDetails(availabilityId, staffMemberId, DateTimeOffset.UtcNow)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/availability/{availabilityId}");
        var request = CreateUpdateAvailabilityRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.UpdateStaffMemberAvailability(staffMemberId, availabilityId, request, cancellationTokenSource.Token);

        handler.Command.Should().Be(new UpdateStaffMemberAvailabilityCommand(
            businessId,
            staffMemberId,
            availabilityId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Theory]
    [InlineData(UpdateStaffMemberAvailabilityError.StaffMemberNotFound, typeof(NotFoundObjectResult), "Staff member not found.")]
    [InlineData(UpdateStaffMemberAvailabilityError.AvailabilityNotFound, typeof(NotFoundObjectResult), "Staff member availability not found.")]
    [InlineData(UpdateStaffMemberAvailabilityError.InvalidDayOfWeek, typeof(BadRequestObjectResult), "Invalid availability day of week.")]
    [InlineData(UpdateStaffMemberAvailabilityError.InvalidTimeRange, typeof(BadRequestObjectResult), "Invalid availability time range.")]
    [InlineData(UpdateStaffMemberAvailabilityError.AvailabilityOverlaps, typeof(ConflictObjectResult), "Staff member availability overlaps.")]
    public async Task UpdateStaffMemberAvailability_WhenHandlerFails_ReturnsProblemDetails(
        UpdateStaffMemberAvailabilityError error,
        Type expectedResultType,
        string expectedTitle)
    {
        var staffMemberId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult>(
            UpdateStaffMemberAvailabilityResult.Failure(error));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability/{availabilityId}");

        var result = await controller.UpdateStaffMemberAvailability(
            staffMemberId,
            availabilityId,
            CreateUpdateAvailabilityRequest(),
            CancellationToken.None);

        result.Result.Should().BeOfType(expectedResultType);
        var objectResult = result.Result.Should().BeAssignableTo<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/availability/{availabilityId}");
    }

    [Fact]
    public async Task DeleteStaffMemberAvailability_WhenAvailabilityExists_ReturnsNoContent()
    {
        var staffMemberId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberAvailabilityCommand, DeleteStaffMemberAvailabilityResult>(
            DeleteStaffMemberAvailabilityResult.Success());
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability/{availabilityId}");

        var result = await controller.DeleteStaffMemberAvailability(staffMemberId, availabilityId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteStaffMemberAvailability_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberAvailabilityCommand, DeleteStaffMemberAvailabilityResult>(
            DeleteStaffMemberAvailabilityResult.Success());
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/availability/{availabilityId}");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.DeleteStaffMemberAvailability(staffMemberId, availabilityId, cancellationTokenSource.Token);

        handler.Command.Should().Be(new DeleteStaffMemberAvailabilityCommand(businessId, staffMemberId, availabilityId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Theory]
    [InlineData(DeleteStaffMemberAvailabilityError.StaffMemberNotFound, "Staff member not found.")]
    [InlineData(DeleteStaffMemberAvailabilityError.AvailabilityNotFound, "Staff member availability not found.")]
    public async Task DeleteStaffMemberAvailability_WhenHandlerFails_ReturnsNotFoundProblemDetails(
        DeleteStaffMemberAvailabilityError error,
        string expectedTitle)
    {
        var staffMemberId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberAvailabilityCommand, DeleteStaffMemberAvailabilityResult>(
            DeleteStaffMemberAvailabilityResult.Failure(error));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/availability/{availabilityId}");

        var result = await controller.DeleteStaffMemberAvailability(staffMemberId, availabilityId, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/availability/{availabilityId}");
    }

    [Fact]
    public async Task UpdateStaffMember_WhenStaffMemberExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult>(
            UpdateStaffMemberResult.Success(new AdminStaffMemberDetails(
                staffMemberId,
                businessId,
                "Laura Premium",
                "laura.premium@example.test",
                "+34600111222",
                "Especialista senior",
                true,
                2,
                createdAtUtc)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}");

        var result = await controller.UpdateStaffMember(staffMemberId, CreateUpdateRequest(), CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StaffMemberResponse>().Subject;
        response.Id.Should().Be(staffMemberId);
        response.BusinessId.Should().Be(businessId);
        response.DisplayName.Should().Be("Laura Premium");
        response.Email.Should().Be("laura.premium@example.test");
        response.PhoneNumber.Should().Be("+34600111222");
        response.Bio.Should().Be("Especialista senior");
        response.IsActive.Should().BeTrue();
        response.SortOrder.Should().Be(2);
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task UpdateStaffMember_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult>(
            UpdateStaffMemberResult.Success(CreateAdminStaffMemberDetails(businessId, staffMemberId)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}");
        var request = CreateUpdateRequest();
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.UpdateStaffMember(staffMemberId, request, cancellationTokenSource.Token);

        handler.Command.Should().NotBeNull();
        handler.Command!.BusinessId.Should().Be(businessId);
        handler.Command.StaffMemberId.Should().Be(staffMemberId);
        handler.Command.DisplayName.Should().Be(request.DisplayName);
        handler.Command.Email.Should().Be(request.Email);
        handler.Command.PhoneNumber.Should().Be(request.PhoneNumber);
        handler.Command.Bio.Should().Be(request.Bio);
        handler.Command.SortOrder.Should().Be(request.SortOrder);
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task UpdateStaffMember_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult>(
            UpdateStaffMemberResult.Success(CreateAdminStaffMemberDetails(Guid.NewGuid(), staffMemberId)));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}");

        var result = await controller.UpdateStaffMember(staffMemberId, CreateUpdateRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStaffMember_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult>(
            UpdateStaffMemberResult.Success(CreateAdminStaffMemberDetails(Guid.NewGuid(), staffMemberId)));
        var controller = CreateController(handler, "not-a-guid", $"/api/staff-members/{staffMemberId}");

        var result = await controller.UpdateStaffMember(staffMemberId, CreateUpdateRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStaffMember_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult>(
            UpdateStaffMemberResult.Failure(UpdateStaffMemberError.StaffMemberNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}");

        var result = await controller.UpdateStaffMember(staffMemberId, CreateUpdateRequest(), CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}");
    }

    [Fact]
    public async Task UpdateStaffMemberActiveState_WhenStaffMemberExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult>(
            UpdateStaffMemberActiveStateResult.Success(CreateAdminStaffMemberDetails(businessId, staffMemberId) with { IsActive = false }));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/active-state");

        var result = await controller.UpdateStaffMemberActiveState(
            staffMemberId,
            new UpdateStaffMemberActiveStateRequest { IsActive = false },
            CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StaffMemberResponse>().Subject;
        response.Id.Should().Be(staffMemberId);
        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStaffMemberActiveState_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult>(
            UpdateStaffMemberActiveStateResult.Success(CreateAdminStaffMemberDetails(businessId, staffMemberId)));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/active-state");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.UpdateStaffMemberActiveState(
            staffMemberId,
            new UpdateStaffMemberActiveStateRequest { IsActive = false },
            cancellationTokenSource.Token);

        handler.Command.Should().Be(new UpdateStaffMemberActiveStateCommand(businessId, staffMemberId, false));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task UpdateStaffMemberActiveState_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult>(
            UpdateStaffMemberActiveStateResult.Success(CreateAdminStaffMemberDetails(Guid.NewGuid(), staffMemberId)));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}/active-state");

        var result = await controller.UpdateStaffMemberActiveState(
            staffMemberId,
            new UpdateStaffMemberActiveStateRequest { IsActive = true },
            CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStaffMemberActiveState_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult>(
            UpdateStaffMemberActiveStateResult.Success(CreateAdminStaffMemberDetails(Guid.NewGuid(), staffMemberId)));
        var controller = CreateController(handler, "not-a-guid", $"/api/staff-members/{staffMemberId}/active-state");

        var result = await controller.UpdateStaffMemberActiveState(
            staffMemberId,
            new UpdateStaffMemberActiveStateRequest { IsActive = true },
            CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStaffMemberActiveState_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult>(
            UpdateStaffMemberActiveStateResult.Failure(UpdateStaffMemberActiveStateError.StaffMemberNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/active-state");

        var result = await controller.UpdateStaffMemberActiveState(
            staffMemberId,
            new UpdateStaffMemberActiveStateRequest { IsActive = true },
            CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/active-state");
    }

    [Fact]
    public async Task DeleteStaffMember_WhenStaffMemberCanBeDeleted_ReturnsNoContent()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult>(DeleteStaffMemberResult.Success());
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}");

        var result = await controller.DeleteStaffMember(staffMemberId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteStaffMember_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult>(DeleteStaffMemberResult.Success());
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.DeleteStaffMember(staffMemberId, cancellationTokenSource.Token);

        handler.Command.Should().Be(new DeleteStaffMemberCommand(businessId, staffMemberId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task DeleteStaffMember_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult>(DeleteStaffMemberResult.Success());
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}");

        var result = await controller.DeleteStaffMember(staffMemberId, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStaffMember_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult>(DeleteStaffMemberResult.Success());
        var controller = CreateController(handler, "not-a-guid", $"/api/staff-members/{staffMemberId}");

        var result = await controller.DeleteStaffMember(staffMemberId, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStaffMember_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult>(
            DeleteStaffMemberResult.Failure(DeleteStaffMemberError.StaffMemberNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}");

        var result = await controller.DeleteStaffMember(staffMemberId, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}");
    }

    [Fact]
    public async Task DeleteStaffMember_WhenStaffMemberHasAppointments_ReturnsConflictProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var handler = new StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult>(
            DeleteStaffMemberResult.Failure(DeleteStaffMemberError.StaffMemberHasAppointments));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}");

        var result = await controller.DeleteStaffMember(staffMemberId, CancellationToken.None);

        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problemDetails = conflictResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Staff member has appointments.");
        problemDetails.Detail.Should().Be("The staff member cannot be deleted because they have appointments. Deactivate them instead.");
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

    [Fact]
    public async Task UpdateStaffMemberServiceActiveState_WhenAssignmentExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult>(
            UpdateStaffMemberServiceActiveStateResult.Success(staffMemberId, serviceId, false, createdAtUtc));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}/active-state");

        var result = await controller.UpdateStaffMemberServiceActiveState(
            staffMemberId,
            serviceId,
            new UpdateStaffMemberServiceActiveStateRequest { IsActive = false },
            CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<StaffMemberServiceAssignmentResponse>().Subject;
        response.StaffMemberId.Should().Be(staffMemberId);
        response.ServiceId.Should().Be(serviceId);
        response.IsActive.Should().BeFalse();
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task UpdateStaffMemberServiceActiveState_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult>(
            UpdateStaffMemberServiceActiveStateResult.Success(staffMemberId, serviceId, false, DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}/active-state");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.UpdateStaffMemberServiceActiveState(
            staffMemberId,
            serviceId,
            new UpdateStaffMemberServiceActiveStateRequest { IsActive = false },
            cancellationTokenSource.Token);

        handler.Command.Should().Be(new UpdateStaffMemberServiceActiveStateCommand(businessId, staffMemberId, serviceId, false));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task UpdateStaffMemberServiceActiveState_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult>(
            UpdateStaffMemberServiceActiveStateResult.Success(staffMemberId, serviceId, true, DateTimeOffset.UtcNow));
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}/services/{serviceId}/active-state");

        var result = await controller.UpdateStaffMemberServiceActiveState(
            staffMemberId,
            serviceId,
            new UpdateStaffMemberServiceActiveStateRequest { IsActive = true },
            CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStaffMemberServiceActiveState_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult>(
            UpdateStaffMemberServiceActiveStateResult.Success(staffMemberId, serviceId, true, DateTimeOffset.UtcNow));
        var controller = CreateController(handler, "not-a-guid", $"/api/staff-members/{staffMemberId}/services/{serviceId}/active-state");

        var result = await controller.UpdateStaffMemberServiceActiveState(
            staffMemberId,
            serviceId,
            new UpdateStaffMemberServiceActiveStateRequest { IsActive = true },
            CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Theory]
    [InlineData(UpdateStaffMemberServiceActiveStateError.StaffMemberNotFound, "Staff member not found.")]
    [InlineData(UpdateStaffMemberServiceActiveStateError.ServiceNotFound, "Service not found.")]
    [InlineData(UpdateStaffMemberServiceActiveStateError.AssignmentNotFound, "Staff member service assignment not found.")]
    public async Task UpdateStaffMemberServiceActiveState_WhenDependencyDoesNotExist_ReturnsNotFoundProblemDetails(
        UpdateStaffMemberServiceActiveStateError error,
        string expectedTitle)
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult>(
            UpdateStaffMemberServiceActiveStateResult.Failure(error));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}/active-state");

        var result = await controller.UpdateStaffMemberServiceActiveState(
            staffMemberId,
            serviceId,
            new UpdateStaffMemberServiceActiveStateRequest { IsActive = false },
            CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/services/{serviceId}/active-state");
    }

    [Fact]
    public async Task UnassignServiceFromStaffMember_WhenAssignmentCanBeDeleted_ReturnsNoContent()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult>(
            UnassignStaffMemberServiceResult.Success());
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.UnassignServiceFromStaffMember(staffMemberId, serviceId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UnassignServiceFromStaffMember_UsesBusinessIdFromAdminToken()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult>(
            UnassignStaffMemberServiceResult.Success());
        var controller = CreateController(handler, businessId.ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.UnassignServiceFromStaffMember(staffMemberId, serviceId, cancellationTokenSource.Token);

        handler.Command.Should().Be(new UnassignStaffMemberServiceCommand(businessId, staffMemberId, serviceId));
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task UnassignServiceFromStaffMember_WhenBusinessClaimIsMissing_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult>(
            UnassignStaffMemberServiceResult.Success());
        var controller = CreateController(handler, businessIdClaimValue: null, $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.UnassignServiceFromStaffMember(staffMemberId, serviceId, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task UnassignServiceFromStaffMember_WhenBusinessClaimIsInvalid_ReturnsForbid()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult>(
            UnassignStaffMemberServiceResult.Success());
        var controller = CreateController(handler, "not-a-guid", $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.UnassignServiceFromStaffMember(staffMemberId, serviceId, CancellationToken.None);

        result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Theory]
    [InlineData(UnassignStaffMemberServiceError.StaffMemberNotFound, "Staff member not found.")]
    [InlineData(UnassignStaffMemberServiceError.ServiceNotFound, "Service not found.")]
    [InlineData(UnassignStaffMemberServiceError.AssignmentNotFound, "Staff member service assignment not found.")]
    public async Task UnassignServiceFromStaffMember_WhenDependencyDoesNotExist_ReturnsNotFoundProblemDetails(
        UnassignStaffMemberServiceError error,
        string expectedTitle)
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult>(
            UnassignStaffMemberServiceResult.Failure(error));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.UnassignServiceFromStaffMember(staffMemberId, serviceId, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/services/{serviceId}");
    }

    [Fact]
    public async Task UnassignServiceFromStaffMember_WhenAssignmentHasAppointments_ReturnsConflictProblemDetails()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult>(
            UnassignStaffMemberServiceResult.Failure(UnassignStaffMemberServiceError.AssignmentHasAppointments));
        var controller = CreateController(handler, Guid.NewGuid().ToString(), $"/api/staff-members/{staffMemberId}/services/{serviceId}");

        var result = await controller.UnassignServiceFromStaffMember(staffMemberId, serviceId, CancellationToken.None);

        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problemDetails = conflictResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Staff member service assignment has appointments.");
        problemDetails.Detail.Should().Be("The staff member service assignment cannot be deleted because it has appointments. Deactivate it instead.");
        problemDetails.Instance.Should().Be($"/api/staff-members/{staffMemberId}/services/{serviceId}");
    }

    private static StaffMembersController CreateController(
        StubCreateStaffMemberHandler handler,
        string? businessIdClaimValue) =>
        CreateController(
            handler,
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
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
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            handler,
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
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
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
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
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            handler,
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            handler,
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            handler,
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            handler,
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            handler,
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            handler,
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            handler,
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubQueryHandler<ListStaffMemberAvailabilitiesQuery, ListStaffMemberAvailabilitiesResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            handler,
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            handler,
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<DeleteStaffMemberAvailabilityCommand, DeleteStaffMemberAvailabilityResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            handler,
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            handler,
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubQueryHandler<ListStaffMemberAvailabilityExceptionsQuery, ListStaffMemberAvailabilityExceptionsResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            handler,
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<UpdateStaffMemberAvailabilityExceptionCommand, UpdateStaffMemberAvailabilityExceptionResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            handler,
            CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCommandHandler<DeleteStaffMemberAvailabilityExceptionCommand, DeleteStaffMemberAvailabilityExceptionResult> handler,
        string? businessIdClaimValue,
        string requestPath) =>
        CreateController(
            CreateDefaultCreateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberHandler(),
            CreateDefaultUpdateStaffMemberActiveStateHandler(),
            CreateDefaultDeleteStaffMemberHandler(),
            CreateDefaultAssignStaffMemberServiceHandler(),
            CreateDefaultUnassignStaffMemberServiceHandler(),
            CreateDefaultUpdateStaffMemberServiceActiveStateHandler(),
            CreateDefaultCreateStaffMemberAvailabilityHandler(),
            CreateDefaultListStaffMemberAvailabilitiesHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityHandler(),
            CreateDefaultDeleteStaffMemberAvailabilityHandler(),
            CreateDefaultCreateStaffMemberAvailabilityExceptionHandler(),
            CreateDefaultListStaffMemberAvailabilityExceptionsHandler(),
            CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler(),
            handler,
            CreateDefaultListAdminStaffMembersHandler(),
            CreateDefaultGetAdminStaffMemberHandler(),
            businessIdClaimValue,
            requestPath);

    private static StaffMembersController CreateController(
        StubCreateStaffMemberHandler createStaffMemberHandler,
        StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult> updateStaffMemberHandler,
        StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult> updateStaffMemberActiveStateHandler,
        StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult> deleteStaffMemberHandler,
        StubAssignStaffMemberServiceHandler assignStaffMemberServiceHandler,
        StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult> unassignStaffMemberServiceHandler,
        StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult> updateStaffMemberServiceActiveStateHandler,
        StubCommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult> createStaffMemberAvailabilityHandler,
        StubQueryHandler<ListStaffMemberAvailabilitiesQuery, ListStaffMemberAvailabilitiesResult> listStaffMemberAvailabilitiesHandler,
        StubCommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult> updateStaffMemberAvailabilityHandler,
        StubCommandHandler<DeleteStaffMemberAvailabilityCommand, DeleteStaffMemberAvailabilityResult> deleteStaffMemberAvailabilityHandler,
        StubCommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult> createStaffMemberAvailabilityExceptionHandler,
        StubQueryHandler<ListStaffMemberAvailabilityExceptionsQuery, ListStaffMemberAvailabilityExceptionsResult> listStaffMemberAvailabilityExceptionsHandler,
        StubCommandHandler<UpdateStaffMemberAvailabilityExceptionCommand, UpdateStaffMemberAvailabilityExceptionResult> updateStaffMemberAvailabilityExceptionHandler,
        StubCommandHandler<DeleteStaffMemberAvailabilityExceptionCommand, DeleteStaffMemberAvailabilityExceptionResult> deleteStaffMemberAvailabilityExceptionHandler,
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
            updateStaffMemberHandler,
            updateStaffMemberActiveStateHandler,
            deleteStaffMemberHandler,
            assignStaffMemberServiceHandler,
            unassignStaffMemberServiceHandler,
            updateStaffMemberServiceActiveStateHandler,
            createStaffMemberAvailabilityHandler,
            listStaffMemberAvailabilitiesHandler,
            updateStaffMemberAvailabilityHandler,
            deleteStaffMemberAvailabilityHandler,
            createStaffMemberAvailabilityExceptionHandler,
            listStaffMemberAvailabilityExceptionsHandler,
            updateStaffMemberAvailabilityExceptionHandler,
            deleteStaffMemberAvailabilityExceptionHandler,
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

    private static UpdateStaffMemberRequest CreateUpdateRequest() => new()
    {
        DisplayName = "Laura Premium",
        Email = "laura.premium@example.test",
        PhoneNumber = "+34600111222",
        Bio = "Especialista senior",
        SortOrder = 2
    };

    private static CreateStaffMemberAvailabilityRequest CreateAvailabilityRequest() => new()
    {
        DayOfWeek = 1,
        StartTime = new TimeOnly(9, 0),
        EndTime = new TimeOnly(13, 0)
    };

    private static UpdateStaffMemberAvailabilityRequest CreateUpdateAvailabilityRequest() => new()
    {
        DayOfWeek = 1,
        StartTime = new TimeOnly(10, 0),
        EndTime = new TimeOnly(14, 0)
    };

    private static CreateStaffMemberAvailabilityExceptionRequest CreateAvailabilityExceptionRequest() => new()
    {
        LocalDate = new DateOnly(2026, 7, 20),
        IsClosed = false,
        StartTime = new TimeOnly(10, 0),
        EndTime = new TimeOnly(14, 0),
        Reason = "Horario especial"
    };

    private static UpdateStaffMemberAvailabilityExceptionRequest CreateUpdateAvailabilityExceptionRequest() => new()
    {
        LocalDate = new DateOnly(2026, 7, 20),
        IsClosed = false,
        StartTime = new TimeOnly(11, 0),
        EndTime = new TimeOnly(15, 0),
        Reason = "Horario especial actualizado"
    };

    private static StaffMemberAvailabilityDetails CreateAvailabilityDetails(
        Guid availabilityId,
        Guid staffMemberId,
        DateTimeOffset createdAtUtc) => new(
            availabilityId,
            staffMemberId,
            1,
            new TimeOnly(9, 0),
            new TimeOnly(13, 0),
            true,
            createdAtUtc);

    private static StaffMemberAvailabilityExceptionDetails CreateAvailabilityExceptionDetails(
        Guid exceptionId,
        Guid staffMemberId,
        DateTimeOffset createdAtUtc) => new(
            exceptionId,
            staffMemberId,
            new DateOnly(2026, 7, 20),
            false,
            new TimeOnly(10, 0),
            new TimeOnly(14, 0),
            "Horario especial",
            createdAtUtc);

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

    private static StubCommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult> CreateDefaultUnassignStaffMemberServiceHandler() => new(
        UnassignStaffMemberServiceResult.Success());

    private static StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult> CreateDefaultUpdateStaffMemberServiceActiveStateHandler()
    {
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        return new StubCommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult>(
            UpdateStaffMemberServiceActiveStateResult.Success(staffMemberId, serviceId, true, DateTimeOffset.UtcNow));
    }

    private static StubCommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult> CreateDefaultCreateStaffMemberAvailabilityHandler()
    {
        var staffMemberId = Guid.NewGuid();
        return new StubCommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult>(
            CreateStaffMemberAvailabilityResult.Success(CreateAvailabilityDetails(Guid.NewGuid(), staffMemberId, DateTimeOffset.UtcNow)));
    }

    private static StubQueryHandler<ListStaffMemberAvailabilitiesQuery, ListStaffMemberAvailabilitiesResult> CreateDefaultListStaffMemberAvailabilitiesHandler() => new(
        ListStaffMemberAvailabilitiesResult.Success([]));

    private static StubCommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult> CreateDefaultUpdateStaffMemberAvailabilityHandler()
    {
        var staffMemberId = Guid.NewGuid();
        return new StubCommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult>(
            UpdateStaffMemberAvailabilityResult.Success(CreateAvailabilityDetails(Guid.NewGuid(), staffMemberId, DateTimeOffset.UtcNow)));
    }

    private static StubCommandHandler<DeleteStaffMemberAvailabilityCommand, DeleteStaffMemberAvailabilityResult> CreateDefaultDeleteStaffMemberAvailabilityHandler() => new(
        DeleteStaffMemberAvailabilityResult.Success());

    private static StubCommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult> CreateDefaultCreateStaffMemberAvailabilityExceptionHandler()
    {
        var staffMemberId = Guid.NewGuid();
        return new StubCommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult>(
            CreateStaffMemberAvailabilityExceptionResult.Success(CreateAvailabilityExceptionDetails(Guid.NewGuid(), staffMemberId, DateTimeOffset.UtcNow)));
    }

    private static StubQueryHandler<ListStaffMemberAvailabilityExceptionsQuery, ListStaffMemberAvailabilityExceptionsResult> CreateDefaultListStaffMemberAvailabilityExceptionsHandler() => new(
        ListStaffMemberAvailabilityExceptionsResult.Success([]));

    private static StubCommandHandler<UpdateStaffMemberAvailabilityExceptionCommand, UpdateStaffMemberAvailabilityExceptionResult> CreateDefaultUpdateStaffMemberAvailabilityExceptionHandler()
    {
        var staffMemberId = Guid.NewGuid();
        return new StubCommandHandler<UpdateStaffMemberAvailabilityExceptionCommand, UpdateStaffMemberAvailabilityExceptionResult>(
            UpdateStaffMemberAvailabilityExceptionResult.Success(CreateAvailabilityExceptionDetails(Guid.NewGuid(), staffMemberId, DateTimeOffset.UtcNow)));
    }

    private static StubCommandHandler<DeleteStaffMemberAvailabilityExceptionCommand, DeleteStaffMemberAvailabilityExceptionResult> CreateDefaultDeleteStaffMemberAvailabilityExceptionHandler() => new(
        DeleteStaffMemberAvailabilityExceptionResult.Success());

    private static StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult> CreateDefaultUpdateStaffMemberHandler()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        return new StubCommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult>(
            UpdateStaffMemberResult.Success(CreateAdminStaffMemberDetails(businessId, staffMemberId)));
    }

    private static StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult> CreateDefaultUpdateStaffMemberActiveStateHandler()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        return new StubCommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult>(
            UpdateStaffMemberActiveStateResult.Success(CreateAdminStaffMemberDetails(businessId, staffMemberId)));
    }

    private static StubCommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult> CreateDefaultDeleteStaffMemberHandler() => new(
        DeleteStaffMemberResult.Success());

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

    private sealed class StubCommandHandler<TCommand, TResult>(TResult result) : ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        public TCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
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
