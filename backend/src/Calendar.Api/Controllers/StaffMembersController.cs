using System.Security.Claims;
using Calendar.Api.Contracts.StaffMemberServices;
using Calendar.Api.Contracts.StaffMembers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberServices.AssignStaffMemberService;
using Calendar.Application.StaffMembers;
using Calendar.Application.StaffMembers.CreateStaffMember;
using Calendar.Application.StaffMembers.DeleteStaffMember;
using Calendar.Application.StaffMembers.UpdateStaffMember;
using Calendar.Application.StaffMembers.UpdateStaffMemberActiveState;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/staff-members")]
public sealed class StaffMembersController(
    ICommandHandler<CreateStaffMemberCommand, CreateStaffMemberResult> createStaffMemberHandler,
    ICommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult> updateStaffMemberHandler,
    ICommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult> updateStaffMemberActiveStateHandler,
    ICommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult> deleteStaffMemberHandler,
    ICommandHandler<AssignStaffMemberServiceCommand, AssignStaffMemberServiceResult> assignStaffMemberServiceHandler,
    IQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult> listAdminStaffMembersHandler,
    IQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult> getAdminStaffMemberHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StaffMemberResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StaffMemberResponse>>> ListStaffMembers(CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await listAdminStaffMembersHandler.HandleAsync(new ListAdminStaffMembersQuery(businessId), cancellationToken);
        if (!result.BusinessFound)
        {
            return NotFound(CreateBusinessNotFoundProblemDetails());
        }

        return Ok(result.StaffMembers.Select(MapStaffMember).ToList());
    }

    [HttpGet("{staffMemberId:guid}")]
    [ProducesResponseType<StaffMemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StaffMemberResponse>> GetStaffMember(
        Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await getAdminStaffMemberHandler.HandleAsync(
            new GetAdminStaffMemberQuery(businessId, staffMemberId),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                GetAdminStaffMemberError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
                GetAdminStaffMemberError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(MapStaffMember(result.StaffMember!));
    }

    [HttpPut("{staffMemberId:guid}")]
    [ProducesResponseType<StaffMemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StaffMemberResponse>> UpdateStaffMember(
        Guid staffMemberId,
        UpdateStaffMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new UpdateStaffMemberCommand(
            businessId,
            staffMemberId,
            request.DisplayName,
            request.Email,
            request.PhoneNumber,
            request.Bio,
            request.SortOrder);

        var result = await updateStaffMemberHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UpdateStaffMemberError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(MapStaffMember(result.StaffMember!));
    }

    [HttpPut("{staffMemberId:guid}/active-state")]
    [ProducesResponseType<StaffMemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StaffMemberResponse>> UpdateStaffMemberActiveState(
        Guid staffMemberId,
        UpdateStaffMemberActiveStateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await updateStaffMemberActiveStateHandler.HandleAsync(
            new UpdateStaffMemberActiveStateCommand(businessId, staffMemberId, request.IsActive),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UpdateStaffMemberActiveStateError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(MapStaffMember(result.StaffMember!));
    }

    [HttpDelete("{staffMemberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteStaffMember(Guid staffMemberId, CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await deleteStaffMemberHandler.HandleAsync(
            new DeleteStaffMemberCommand(businessId, staffMemberId),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                DeleteStaffMemberError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                DeleteStaffMemberError.StaffMemberHasAppointments => Conflict(CreateStaffMemberHasAppointmentsProblemDetails()),
                _ => BadRequest()
            };
        }

        return NoContent();
    }

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

    [HttpPost("{staffMemberId:guid}/services/{serviceId:guid}")]
    [ProducesResponseType<StaffMemberServiceAssignmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffMemberServiceAssignmentResponse>> AssignServiceToStaffMember(
        Guid staffMemberId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new AssignStaffMemberServiceCommand(businessId, staffMemberId, serviceId);

        var result = await assignStaffMemberServiceHandler.HandleAsync(command, cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                AssignStaffMemberServiceError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                AssignStaffMemberServiceError.ServiceNotFound => NotFound(CreateServiceNotFoundProblemDetails()),
                AssignStaffMemberServiceError.AssignmentAlreadyExists => Conflict(CreateAssignmentAlreadyExistsProblemDetails()),
                _ => BadRequest()
            };
        }

        var response = new StaffMemberServiceAssignmentResponse(
            result.StaffMemberId!.Value,
            result.ServiceId!.Value,
            result.IsActive!.Value,
            result.CreatedAtUtc!.Value);

        return StatusCode(StatusCodes.Status201Created, response);
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

    private ProblemDetails CreateStaffMemberNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Staff member not found.",
        Detail = "The staff member was not found for the current admin account business.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateServiceNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Service not found.",
        Detail = "The service was not found for the current admin account business.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAssignmentAlreadyExistsProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Staff member service assignment already exists.",
        Detail = "The staff member is already assigned to this service.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateStaffMemberHasAppointmentsProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Staff member has appointments.",
        Detail = "The staff member cannot be deleted because they have appointments. Deactivate them instead.",
        Instance = HttpContext.Request.Path
    };

    private static StaffMemberResponse MapStaffMember(AdminStaffMemberDetails staffMember) => new(
        staffMember.Id,
        staffMember.BusinessId,
        staffMember.DisplayName,
        staffMember.Email,
        staffMember.PhoneNumber,
        staffMember.Bio,
        staffMember.IsActive,
        staffMember.SortOrder,
        staffMember.CreatedAtUtc);
}
