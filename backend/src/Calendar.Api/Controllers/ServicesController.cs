using System.Security.Claims;
using Calendar.Api.Contracts.StaffMemberServices;
using Calendar.Api.Contracts.Services;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Services;
using Calendar.Application.Services.CreateService;
using Calendar.Application.Services.DeleteService;
using Calendar.Application.Services.UpdateService;
using Calendar.Application.Services.UpdateServiceActiveState;
using Calendar.Application.StaffMemberServices.AssignStaffMemberService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/services")]
public sealed class ServicesController(
    ICommandHandler<CreateServiceCommand, CreateServiceResult> createServiceHandler,
    ICommandHandler<UpdateServiceCommand, UpdateServiceResult> updateServiceHandler,
    ICommandHandler<UpdateServiceActiveStateCommand, UpdateServiceActiveStateResult> updateServiceActiveStateHandler,
    ICommandHandler<DeleteServiceCommand, DeleteServiceResult> deleteServiceHandler,
    ICommandHandler<AssignStaffMemberServiceCommand, AssignStaffMemberServiceResult> assignStaffMemberServiceHandler,
    IQueryHandler<ListAdminServicesQuery, ListAdminServicesResult> listAdminServicesHandler,
    IQueryHandler<GetAdminServiceQuery, GetAdminServiceResult> getAdminServiceHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> ListServices(CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await listAdminServicesHandler.HandleAsync(new ListAdminServicesQuery(businessId), cancellationToken);
        if (!result.BusinessFound)
        {
            return NotFound(CreateBusinessNotFoundProblemDetails());
        }

        return Ok(result.Services.Select(MapService).ToList());
    }

    [HttpGet("{serviceId:guid}")]
    [ProducesResponseType<ServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse>> GetService(
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await getAdminServiceHandler.HandleAsync(new GetAdminServiceQuery(businessId, serviceId), cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                GetAdminServiceError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
                GetAdminServiceError.ServiceNotFound => NotFound(CreateServiceNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(MapService(result.Service!));
    }

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

    [HttpPut("{serviceId:guid}")]
    [ProducesResponseType<ServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse>> UpdateService(
        Guid serviceId,
        UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new UpdateServiceCommand(
            businessId,
            serviceId,
            request.Name,
            request.Description,
            request.DurationMinutes,
            request.PriceAmount,
            request.SortOrder);

        var result = await updateServiceHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UpdateServiceError.ServiceNotFound => NotFound(CreateServiceNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(MapService(result.Service!));
    }

    [HttpPut("{serviceId:guid}/active-state")]
    [ProducesResponseType<ServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse>> UpdateServiceActiveState(
        Guid serviceId,
        UpdateServiceActiveStateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await updateServiceActiveStateHandler.HandleAsync(
            new UpdateServiceActiveStateCommand(businessId, serviceId, request.IsActive),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UpdateServiceActiveStateError.ServiceNotFound => NotFound(CreateServiceNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(MapService(result.Service!));
    }

    [HttpDelete("{serviceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteService(Guid serviceId, CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await deleteServiceHandler.HandleAsync(
            new DeleteServiceCommand(businessId, serviceId),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                DeleteServiceError.ServiceNotFound => NotFound(CreateServiceNotFoundProblemDetails()),
                DeleteServiceError.ServiceHasAppointments => Conflict(CreateServiceHasAppointmentsProblemDetails()),
                _ => BadRequest()
            };
        }

        return NoContent();
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

    private ProblemDetails CreateServiceHasAppointmentsProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Service has appointments.",
        Detail = "The service cannot be deleted because it has appointments. Deactivate it instead.",
        Instance = HttpContext.Request.Path
    };

    private static ServiceResponse MapService(AdminServiceDetails service) => new(
        service.Id,
        service.BusinessId,
        service.Name,
        service.Description,
        service.DurationMinutes,
        service.PriceAmount,
        service.IsActive,
        service.SortOrder,
        service.CreatedAtUtc);
}
