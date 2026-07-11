using System.Security.Claims;
using Calendar.Api.Contracts.StaffMemberServices;
using Calendar.Api.Contracts.Services;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Services.CreateService;
using Calendar.Application.StaffMemberServices.AssignStaffMemberService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/services")]
public sealed class ServicesController(
    ICommandHandler<CreateServiceCommand, CreateServiceResult> createServiceHandler,
    ICommandHandler<AssignStaffMemberServiceCommand, AssignStaffMemberServiceResult> assignStaffMemberServiceHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ServiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse>> CreateService(
        CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new CreateServiceCommand(
            businessId,
            request.Name,
            request.Description,
            request.DurationMinutes,
            request.PriceAmount,
            request.SortOrder);

        var result = await createServiceHandler.HandleAsync(command, cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                CreateServiceError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        var response = new ServiceResponse(
            result.ServiceId!.Value,
            result.BusinessId!.Value,
            result.Name!,
            result.Description,
            result.DurationMinutes!.Value,
            result.PriceAmount!.Value,
            result.IsActive!.Value,
            result.SortOrder!.Value,
            result.CreatedAtUtc!.Value);

        return Created($"/api/services/{response.Id}", response);
    }

    [HttpPost("{serviceId:guid}/staff-members/{staffMemberId:guid}")]
    [ProducesResponseType<StaffMemberServiceAssignmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffMemberServiceAssignmentResponse>> AssignStaffMemberToService(
        Guid serviceId,
        Guid staffMemberId,
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
}
