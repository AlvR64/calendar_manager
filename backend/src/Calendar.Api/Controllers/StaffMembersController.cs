using System.Security.Claims;
using Calendar.Api.Contracts.StaffMembers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMembers.CreateStaffMember;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/staff-members")]
public sealed class StaffMembersController(
    ICommandHandler<CreateStaffMemberCommand, CreateStaffMemberResult> createStaffMemberHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<StaffMemberResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StaffMemberResponse>> CreateStaffMember(
        CreateStaffMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new CreateStaffMemberCommand(
            businessId,
            request.DisplayName,
            request.Email,
            request.PhoneNumber,
            request.Bio,
            request.SortOrder);

        var result = await createStaffMemberHandler.HandleAsync(command, cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                CreateStaffMemberError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        var response = new StaffMemberResponse(
            result.StaffMemberId!.Value,
            result.BusinessId!.Value,
            result.DisplayName!,
            result.Email,
            result.PhoneNumber,
            result.Bio,
            result.IsActive!.Value,
            result.SortOrder!.Value,
            result.CreatedAtUtc!.Value);

        return Created($"/api/staff-members/{response.Id}", response);
    }

    private bool TryGetBusinessId(out Guid businessId)
    {
        var businessIdValue = User.FindFirstValue("business_id");
        return Guid.TryParse(businessIdValue, out businessId);
    }

    private ProblemDetails CreateBusinessNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Business not found.",
        Detail = "The business associated with the current admin account was not found.",
        Instance = HttpContext.Request.Path
    };
}
