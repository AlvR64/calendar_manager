using System.Security.Claims;
using Calendar.Api.Contracts.StaffMembers;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
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

    private static StaffMembersController CreateController(
        StubCreateStaffMemberHandler handler,
        string? businessIdClaimValue)
    {
        var claims = new List<Claim>();
        if (businessIdClaimValue is not null)
        {
            claims.Add(new Claim("business_id", businessIdClaimValue));
        }

        return new StaffMembersController(handler)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")),
                    Request =
                    {
                        Path = "/api/staff-members"
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
}
