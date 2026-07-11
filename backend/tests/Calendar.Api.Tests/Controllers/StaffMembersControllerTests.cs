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

        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal($"/api/staff-members/{staffMemberId}", createdResult.Location);

        var response = Assert.IsType<StaffMemberResponse>(createdResult.Value);
        Assert.Equal(staffMemberId, response.Id);
        Assert.Equal(businessId, response.BusinessId);
        Assert.Equal("Laura Martinez", response.DisplayName);
        Assert.Equal("laura@example.test", response.Email);
        Assert.Equal("+34600999888", response.PhoneNumber);
        Assert.Equal("Especialista en cortes y color", response.Bio);
        Assert.True(response.IsActive);
        Assert.Equal(0, response.SortOrder);
        Assert.Equal(createdAtUtc, response.CreatedAtUtc);
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

        Assert.NotNull(handler.Command);
        Assert.Equal(businessId, handler.Command.BusinessId);
        Assert.Equal(request.DisplayName, handler.Command.DisplayName);
        Assert.Equal(request.Email, handler.Command.Email);
        Assert.Equal(request.PhoneNumber, handler.Command.PhoneNumber);
        Assert.Equal(request.Bio, handler.Command.Bio);
        Assert.Equal(request.SortOrder, handler.Command.SortOrder);
        Assert.Equal(cancellationTokenSource.Token, handler.CancellationToken);
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

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Null(handler.Command);
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

        Assert.IsType<ForbidResult>(result.Result);
        Assert.Null(handler.Command);
    }

    [Fact]
    public async Task CreateStaffMember_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubCreateStaffMemberHandler(
            CreateStaffMemberResult.Failure(CreateStaffMemberError.BusinessNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString());

        var result = await controller.CreateStaffMember(CreateRequest(), CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("Business not found.", problemDetails.Title);
        Assert.Equal("/api/staff-members", problemDetails.Instance);
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
