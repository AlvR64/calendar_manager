using System.Security.Claims;
using Calendar.Api.Contracts.StaffMemberServices;
using Calendar.Api.Contracts.StaffMembers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilities;
using Calendar.Application.StaffMemberAvailabilities.CreateStaffMemberAvailability;
using Calendar.Application.StaffMemberAvailabilities.DeleteStaffMemberAvailability;
using Calendar.Application.StaffMemberAvailabilities.ListStaffMemberAvailabilities;
using Calendar.Application.StaffMemberAvailabilities.UpdateStaffMemberAvailability;
using Calendar.Application.StaffMemberAvailabilityExceptions;
using Calendar.Application.StaffMemberAvailabilityExceptions.CreateStaffMemberAvailabilityException;
using Calendar.Application.StaffMemberAvailabilityExceptions.ListStaffMemberAvailabilityExceptions;
using Calendar.Application.StaffMemberServices.AssignStaffMemberService;
using Calendar.Application.StaffMemberServices.UnassignStaffMemberService;
using Calendar.Application.StaffMemberServices.UpdateStaffMemberServiceActiveState;
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
    ICommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult> unassignStaffMemberServiceHandler,
    ICommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult> updateStaffMemberServiceActiveStateHandler,
    ICommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult> createStaffMemberAvailabilityHandler,
    IQueryHandler<ListStaffMemberAvailabilitiesQuery, ListStaffMemberAvailabilitiesResult> listStaffMemberAvailabilitiesHandler,
    ICommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult> updateStaffMemberAvailabilityHandler,
    ICommandHandler<DeleteStaffMemberAvailabilityCommand, DeleteStaffMemberAvailabilityResult> deleteStaffMemberAvailabilityHandler,
    ICommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult> createStaffMemberAvailabilityExceptionHandler,
    IQueryHandler<ListStaffMemberAvailabilityExceptionsQuery, ListStaffMemberAvailabilityExceptionsResult> listStaffMemberAvailabilityExceptionsHandler,
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

    [HttpGet("{staffMemberId:guid}/availability")]
    [ProducesResponseType<IReadOnlyList<StaffMemberAvailabilityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StaffMemberAvailabilityResponse>>> ListStaffMemberAvailabilities(
        Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await listStaffMemberAvailabilitiesHandler.HandleAsync(
            new ListStaffMemberAvailabilitiesQuery(businessId, staffMemberId),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                ListStaffMemberAvailabilitiesError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(result.Availabilities.Select(MapAvailability).ToList());
    }

    [HttpPost("{staffMemberId:guid}/availability")]
    [ProducesResponseType<StaffMemberAvailabilityResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffMemberAvailabilityResponse>> CreateStaffMemberAvailability(
        Guid staffMemberId,
        CreateStaffMemberAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new CreateStaffMemberAvailabilityCommand(
            businessId,
            staffMemberId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime);

        var result = await createStaffMemberAvailabilityHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                CreateStaffMemberAvailabilityError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                CreateStaffMemberAvailabilityError.InvalidDayOfWeek => BadRequest(CreateInvalidAvailabilityDayOfWeekProblemDetails()),
                CreateStaffMemberAvailabilityError.InvalidTimeRange => BadRequest(CreateInvalidAvailabilityTimeRangeProblemDetails()),
                CreateStaffMemberAvailabilityError.AvailabilityOverlaps => Conflict(CreateAvailabilityOverlapsProblemDetails()),
                _ => BadRequest()
            };
        }

        var response = MapAvailability(result.Availability!);
        return Created($"/api/staff-members/{staffMemberId}/availability/{response.Id}", response);
    }

    [HttpGet("{staffMemberId:guid}/availability-exceptions")]
    [ProducesResponseType<IReadOnlyList<StaffMemberAvailabilityExceptionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StaffMemberAvailabilityExceptionResponse>>> ListStaffMemberAvailabilityExceptions(
        Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await listStaffMemberAvailabilityExceptionsHandler.HandleAsync(
            new ListStaffMemberAvailabilityExceptionsQuery(businessId, staffMemberId),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                ListStaffMemberAvailabilityExceptionsError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(result.Exceptions.Select(MapAvailabilityException).ToList());
    }

    [HttpPost("{staffMemberId:guid}/availability-exceptions")]
    [ProducesResponseType<StaffMemberAvailabilityExceptionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffMemberAvailabilityExceptionResponse>> CreateStaffMemberAvailabilityException(
        Guid staffMemberId,
        CreateStaffMemberAvailabilityExceptionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new CreateStaffMemberAvailabilityExceptionCommand(
            businessId,
            staffMemberId,
            request.LocalDate,
            request.IsClosed,
            request.StartTime,
            request.EndTime,
            request.Reason);

        var result = await createStaffMemberAvailabilityExceptionHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                CreateStaffMemberAvailabilityExceptionError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                CreateStaffMemberAvailabilityExceptionError.InvalidClosedException => BadRequest(CreateInvalidClosedAvailabilityExceptionProblemDetails()),
                CreateStaffMemberAvailabilityExceptionError.InvalidTimeRange => BadRequest(CreateInvalidAvailabilityExceptionTimeRangeProblemDetails()),
                CreateStaffMemberAvailabilityExceptionError.ReasonTooLong => BadRequest(CreateAvailabilityExceptionReasonTooLongProblemDetails()),
                CreateStaffMemberAvailabilityExceptionError.AvailabilityExceptionAlreadyExists => Conflict(CreateAvailabilityExceptionAlreadyExistsProblemDetails()),
                CreateStaffMemberAvailabilityExceptionError.AvailabilityExceptionOverlaps => Conflict(CreateAvailabilityExceptionOverlapsProblemDetails()),
                _ => BadRequest()
            };
        }

        var response = MapAvailabilityException(result.Exception!);
        return Created($"/api/staff-members/{staffMemberId}/availability-exceptions/{response.Id}", response);
    }

    [HttpPut("{staffMemberId:guid}/availability/{availabilityId:guid}")]
    [ProducesResponseType<StaffMemberAvailabilityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffMemberAvailabilityResponse>> UpdateStaffMemberAvailability(
        Guid staffMemberId,
        Guid availabilityId,
        UpdateStaffMemberAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new UpdateStaffMemberAvailabilityCommand(
            businessId,
            staffMemberId,
            availabilityId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime);

        var result = await updateStaffMemberAvailabilityHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UpdateStaffMemberAvailabilityError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                UpdateStaffMemberAvailabilityError.AvailabilityNotFound => NotFound(CreateAvailabilityNotFoundProblemDetails()),
                UpdateStaffMemberAvailabilityError.InvalidDayOfWeek => BadRequest(CreateInvalidAvailabilityDayOfWeekProblemDetails()),
                UpdateStaffMemberAvailabilityError.InvalidTimeRange => BadRequest(CreateInvalidAvailabilityTimeRangeProblemDetails()),
                UpdateStaffMemberAvailabilityError.AvailabilityOverlaps => Conflict(CreateAvailabilityOverlapsProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(MapAvailability(result.Availability!));
    }

    [HttpDelete("{staffMemberId:guid}/availability/{availabilityId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStaffMemberAvailability(
        Guid staffMemberId,
        Guid availabilityId,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await deleteStaffMemberAvailabilityHandler.HandleAsync(
            new DeleteStaffMemberAvailabilityCommand(businessId, staffMemberId, availabilityId),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                DeleteStaffMemberAvailabilityError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                DeleteStaffMemberAvailabilityError.AvailabilityNotFound => NotFound(CreateAvailabilityNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return NoContent();
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

    [HttpPut("{staffMemberId:guid}/services/{serviceId:guid}/active-state")]
    [ProducesResponseType<StaffMemberServiceAssignmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StaffMemberServiceAssignmentResponse>> UpdateStaffMemberServiceActiveState(
        Guid staffMemberId,
        Guid serviceId,
        UpdateStaffMemberServiceActiveStateRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new UpdateStaffMemberServiceActiveStateCommand(
            businessId,
            staffMemberId,
            serviceId,
            request.IsActive);

        var result = await updateStaffMemberServiceActiveStateHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UpdateStaffMemberServiceActiveStateError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                UpdateStaffMemberServiceActiveStateError.ServiceNotFound => NotFound(CreateServiceNotFoundProblemDetails()),
                UpdateStaffMemberServiceActiveStateError.AssignmentNotFound => NotFound(CreateAssignmentNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        var response = new StaffMemberServiceAssignmentResponse(
            result.StaffMemberId!.Value,
            result.ServiceId!.Value,
            result.IsActive!.Value,
            result.CreatedAtUtc!.Value);

        return Ok(response);
    }

    [HttpDelete("{staffMemberId:guid}/services/{serviceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UnassignServiceFromStaffMember(
        Guid staffMemberId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await unassignStaffMemberServiceHandler.HandleAsync(
            new UnassignStaffMemberServiceCommand(businessId, staffMemberId, serviceId),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UnassignStaffMemberServiceError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
                UnassignStaffMemberServiceError.ServiceNotFound => NotFound(CreateServiceNotFoundProblemDetails()),
                UnassignStaffMemberServiceError.AssignmentNotFound => NotFound(CreateAssignmentNotFoundProblemDetails()),
                UnassignStaffMemberServiceError.AssignmentHasAppointments => Conflict(CreateAssignmentHasAppointmentsProblemDetails()),
                _ => BadRequest()
            };
        }

        return NoContent();
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

    private ProblemDetails CreateAssignmentNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Staff member service assignment not found.",
        Detail = "The staff member is not assigned to this service.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAssignmentHasAppointmentsProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Staff member service assignment has appointments.",
        Detail = "The staff member service assignment cannot be deleted because it has appointments. Deactivate it instead.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateStaffMemberHasAppointmentsProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Staff member has appointments.",
        Detail = "The staff member cannot be deleted because they have appointments. Deactivate them instead.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAvailabilityNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Staff member availability not found.",
        Detail = "The availability block was not found for this staff member.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidAvailabilityDayOfWeekProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid availability day of week.",
        Detail = "The availability day of week must be between 0 and 6.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidAvailabilityTimeRangeProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid availability time range.",
        Detail = "The availability end time must be after the start time.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAvailabilityOverlapsProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Staff member availability overlaps.",
        Detail = "The availability block overlaps with an existing block for this staff member and day.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidClosedAvailabilityExceptionProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid closed availability exception.",
        Detail = "A closed availability exception must not include a start or end time.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidAvailabilityExceptionTimeRangeProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid availability exception time range.",
        Detail = "An open availability exception must include an end time after the start time.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAvailabilityExceptionReasonTooLongProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Availability exception reason is too long.",
        Detail = "The availability exception reason must be 250 characters or fewer.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAvailabilityExceptionAlreadyExistsProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Staff member availability exception already exists for this date.",
        Detail = "The staff member already has an availability exception for this date that conflicts with the requested exception.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAvailabilityExceptionOverlapsProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Staff member availability exception overlaps.",
        Detail = "The availability exception overlaps with an existing open exception for this staff member and date.",
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

    private static StaffMemberAvailabilityResponse MapAvailability(StaffMemberAvailabilityDetails availability) => new(
        availability.Id,
        availability.StaffMemberId,
        availability.DayOfWeek,
        availability.StartTime,
        availability.EndTime,
        availability.IsActive,
        availability.CreatedAtUtc);

    private static StaffMemberAvailabilityExceptionResponse MapAvailabilityException(StaffMemberAvailabilityExceptionDetails exception) => new(
        exception.Id,
        exception.StaffMemberId,
        exception.LocalDate,
        exception.IsClosed,
        exception.StartTime,
        exception.EndTime,
        exception.Reason,
        exception.CreatedAtUtc);
}
