using System.Security.Claims;
using Calendar.Api.Contracts.Appointments;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.CancelAdminAppointment;
using Calendar.Application.Appointments.CreateAppointment;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Application.Appointments.ListAdminAppointments;
using Calendar.Application.Appointments.UpdateAppointmentInternalNotes;
using Calendar.Application.Appointments.UpdateAppointmentStatus;
using Calendar.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController(
    ICommandHandler<CreateAppointmentCommand, CreateAppointmentResult> createAppointmentHandler,
    ICommandHandler<CancelAdminAppointmentCommand, CancelAdminAppointmentResult> cancelAdminAppointmentHandler,
    ICommandHandler<UpdateAppointmentStatusCommand, UpdateAppointmentStatusResult> updateAppointmentStatusHandler,
    ICommandHandler<UpdateAppointmentInternalNotesCommand, UpdateAppointmentInternalNotesResult> updateAppointmentInternalNotesHandler,
    IQueryHandler<GetAppointmentDetailsQuery, AppointmentDetails?> getAppointmentDetailsHandler,
    IQueryHandler<ListAdminAppointmentsQuery, ListAdminAppointmentsResult> listAdminAppointmentsHandler) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType<AppointmentSummaryResponse[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AppointmentSummaryResponse>>> ListAppointments(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? staffMemberId,
        [FromQuery] Guid? serviceId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        if (!TryGetAdminBusinessId(out var businessId))
        {
            return Forbid();
        }

        if (!from.HasValue || !to.HasValue)
        {
            return BadRequest(CreateMissingDateRangeProblemDetails());
        }

        if (!TryParseStatus(status, out var appointmentStatus))
        {
            return BadRequest(CreateInvalidStatusProblemDetails());
        }

        var result = await listAdminAppointmentsHandler.HandleAsync(
            new ListAdminAppointmentsQuery(businessId, from.Value, to.Value, staffMemberId, serviceId, appointmentStatus),
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToActionResult(result.Error!.Value);
        }

        return Ok(result.Appointments.Select(MapAppointmentSummary).ToList());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{appointmentId:guid}/cancel")]
    [ProducesResponseType<AppointmentDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDetailsResponse>> CancelAppointmentAsAdmin(
        Guid appointmentId,
        CancelAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAdminBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await cancelAdminAppointmentHandler.HandleAsync(
            new CancelAdminAppointmentCommand(businessId, appointmentId, request.CancellationReason),
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToActionResult(result.Error!.Value);
        }

        return Ok(MapAppointmentDetails(result.Appointment!));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{appointmentId:guid}/status")]
    [ProducesResponseType<AppointmentDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDetailsResponse>> UpdateAppointmentStatus(
        Guid appointmentId,
        UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAdminBusinessId(out var businessId))
        {
            return Forbid();
        }

        if (!TryParseOperationalStatus(request.Status, out var status))
        {
            return BadRequest(CreateInvalidOperationalStatusProblemDetails());
        }

        var result = await updateAppointmentStatusHandler.HandleAsync(
            new UpdateAppointmentStatusCommand(businessId, appointmentId, status),
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToActionResult(result.Error!.Value);
        }

        return Ok(MapAppointmentDetails(result.Appointment!));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{appointmentId:guid}/internal-notes")]
    [ProducesResponseType<AppointmentDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDetailsResponse>> UpdateAppointmentInternalNotes(
        Guid appointmentId,
        UpdateAppointmentInternalNotesRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAdminBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await updateAppointmentInternalNotesHandler.HandleAsync(
            new UpdateAppointmentInternalNotesCommand(businessId, appointmentId, request.InternalNotes),
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToActionResult(result.Error!.Value);
        }

        return Ok(MapAppointmentDetails(result.Appointment!));
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("{appointmentId:guid}")]
    [ProducesResponseType<AppointmentDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDetailsResponse>> GetAppointment(
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var details = await getAppointmentDetailsHandler.HandleAsync(new GetAppointmentDetailsQuery(appointmentId), cancellationToken);
        if (details is null)
        {
            return NotFound(CreateAppointmentNotFoundProblemDetails());
        }

        if (!CanAccess(details))
        {
            return Forbid();
        }

        return Ok(MapAppointmentDetails(details));
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        var command = new CreateAppointmentCommand(
            customerId,
            request.BusinessId,
            request.ServiceId,
            request.StaffMemberId,
            request.StartAtUtc,
            request.CustomerNotes);

        var result = await createAppointmentHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return ToActionResult(result.Error);
        }

        var response = MapAppointment(result);

        return Created($"/api/appointments/{response.Id}", response);
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var customerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(customerIdValue, out customerId);
    }

    private bool TryGetAdminBusinessId(out Guid businessId)
    {
        var businessIdValue = User.FindFirstValue("business_id");
        return Guid.TryParse(businessIdValue, out businessId);
    }

    private bool CanAccess(AppointmentDetails details)
    {
        if (User.IsInRole("Customer") && TryGetCustomerId(out var customerId))
        {
            return details.Customer.Id == customerId;
        }

        if (User.IsInRole("Admin") && TryGetAdminBusinessId(out var businessId))
        {
            return details.Business.Id == businessId;
        }

        return false;
    }

    private static bool TryParseStatus(string? value, out AppointmentStatus? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!Enum.TryParse<AppointmentStatus>(value, ignoreCase: true, out var parsedStatus))
        {
            return false;
        }

        status = parsedStatus;
        return true;
    }

    private static bool TryParseOperationalStatus(string? value, out AppointmentStatus status)
    {
        if (!Enum.TryParse<AppointmentStatus>(value, ignoreCase: true, out var parsedStatus))
        {
            status = default;
            return false;
        }

        status = parsedStatus;
        return status is AppointmentStatus.Scheduled or AppointmentStatus.Completed or AppointmentStatus.NoShow;
    }

    private ActionResult ToActionResult(CreateAppointmentError error) => error switch
    {
        CreateAppointmentError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
        CreateAppointmentError.ServiceNotFound => NotFound(CreateServiceNotFoundProblemDetails()),
        CreateAppointmentError.StaffMemberNotFound => NotFound(CreateStaffMemberNotFoundProblemDetails()),
        CreateAppointmentError.StaffMemberServiceAssignmentNotFound => NotFound(CreateAssignmentNotFoundProblemDetails()),
        CreateAppointmentError.AppointmentOverlaps => Conflict(CreateAppointmentConflictProblemDetails()),
        CreateAppointmentError.ConcurrentSlotConflict => Conflict(CreateAppointmentConflictProblemDetails()),
        CreateAppointmentError.InvalidBusinessTimeZone => BadRequest(CreateInvalidBusinessTimeZoneProblemDetails()),
        CreateAppointmentError.InvalidTimeRange => BadRequest(CreateInvalidTimeRangeProblemDetails()),
        CreateAppointmentError.OutsideBookingWindow => BadRequest(CreateOutsideBookingWindowProblemDetails()),
        CreateAppointmentError.OutsideAvailability => BadRequest(CreateOutsideAvailabilityProblemDetails()),
        _ => BadRequest()
    };

    private ActionResult ToActionResult(ListAdminAppointmentsError error) => error switch
    {
        ListAdminAppointmentsError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
        ListAdminAppointmentsError.InvalidBusinessTimeZone => BadRequest(CreateInvalidBusinessTimeZoneProblemDetails()),
        ListAdminAppointmentsError.InvalidDateRange => BadRequest(CreateInvalidDateRangeProblemDetails()),
        ListAdminAppointmentsError.DateRangeTooLarge => BadRequest(CreateDateRangeTooLargeProblemDetails()),
        _ => BadRequest()
    };

    private ActionResult ToActionResult(CancelAdminAppointmentError error) => error switch
    {
        CancelAdminAppointmentError.AppointmentNotFound => NotFound(CreateAppointmentNotFoundProblemDetails()),
        CancelAdminAppointmentError.Forbidden => Forbid(),
        CancelAdminAppointmentError.AlreadyCancelled => Conflict(CreateAppointmentAlreadyCancelledProblemDetails()),
        _ => BadRequest()
    };

    private ActionResult ToActionResult(UpdateAppointmentStatusError error) => error switch
    {
        UpdateAppointmentStatusError.AppointmentNotFound => NotFound(CreateAppointmentNotFoundProblemDetails()),
        UpdateAppointmentStatusError.Forbidden => Forbid(),
        UpdateAppointmentStatusError.InvalidStatus => BadRequest(CreateInvalidOperationalStatusProblemDetails()),
        UpdateAppointmentStatusError.AppointmentCancelled => Conflict(CreateAppointmentAlreadyCancelledProblemDetails()),
        _ => BadRequest()
    };

    private ActionResult ToActionResult(UpdateAppointmentInternalNotesError error) => error switch
    {
        UpdateAppointmentInternalNotesError.AppointmentNotFound => NotFound(CreateAppointmentNotFoundProblemDetails()),
        UpdateAppointmentInternalNotesError.Forbidden => Forbid(),
        UpdateAppointmentInternalNotesError.InternalNotesTooLong => BadRequest(CreateInternalNotesTooLongProblemDetails()),
        _ => BadRequest()
    };

    private ProblemDetails CreateBusinessNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Business not found.",
        Detail = "The business was not found or is not active.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAppointmentNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Appointment not found.",
        Detail = "The appointment was not found.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateMissingDateRangeProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Appointment date range is required.",
        Detail = "The from and to query parameters are required and must be local dates.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidStatusProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid appointment status.",
        Detail = "The status filter must be a valid appointment status.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidOperationalStatusProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid appointment status.",
        Detail = "The appointment status must be Scheduled, Completed, or NoShow.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAppointmentAlreadyCancelledProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Appointment already cancelled.",
        Detail = "The appointment is already cancelled.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInternalNotesTooLongProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Appointment internal notes are too long.",
        Detail = "Appointment internal notes must be 1000 characters or fewer.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidDateRangeProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid appointment date range.",
        Detail = "The to date must be on or after the from date.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateDateRangeTooLargeProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Appointment date range is too large.",
        Detail = "The appointment date range must be 90 days or fewer.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateServiceNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Service not found.",
        Detail = "The service was not found for this business or is not active.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateStaffMemberNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Staff member not found.",
        Detail = "The staff member was not found for this business or is not active.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAssignmentNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Staff member service assignment not found.",
        Detail = "The staff member is not assigned to this service for this business or the assignment is not active.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAppointmentConflictProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Appointment slot unavailable.",
        Detail = "The selected appointment slot is no longer available.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidBusinessTimeZoneProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid business time zone.",
        Detail = "The business time zone id must be a valid IANA time zone id.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidTimeRangeProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid appointment time range.",
        Detail = "The appointment time range is invalid.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateOutsideBookingWindowProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Appointment outside booking window.",
        Detail = "The appointment must be within the business booking window.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateOutsideAvailabilityProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Appointment outside availability.",
        Detail = "The appointment must fit within staff member availability and outside exceptions.",
        Instance = HttpContext.Request.Path
    };

    private static AppointmentResponse MapAppointment(CreateAppointmentResult result) => new(
        result.AppointmentId!.Value,
        result.BusinessId!.Value,
        result.StaffMemberId!.Value,
        result.ServiceId!.Value,
        result.CustomerId!.Value,
        result.StartAtUtc!.Value,
        result.EndAtUtc!.Value,
        result.Status!,
        result.CustomerNotes,
        result.ServiceNameSnapshot!,
        result.ServiceDurationMinutesSnapshot!.Value,
        result.PriceAmountSnapshot!.Value,
        result.CurrencyCodeSnapshot!,
        result.CreatedAtUtc!.Value);

    private static AppointmentDetailsResponse MapAppointmentDetails(AppointmentDetails details) => new(
        details.Id,
        new AppointmentBusinessResponse(
            details.Business.Id,
            details.Business.Name,
            details.Business.Slug,
            details.Business.TimeZoneId),
        new AppointmentServiceResponse(
            details.Service.Id,
            details.Service.NameSnapshot,
            details.Service.DurationMinutesSnapshot,
            details.Service.PriceAmountSnapshot,
            details.Service.CurrencyCodeSnapshot),
        new AppointmentStaffMemberResponse(
            details.StaffMember.Id,
            details.StaffMember.DisplayName),
        new AppointmentCustomerResponse(
            details.Customer.Id,
            details.Customer.FirstName,
            details.Customer.LastName,
            details.Customer.Email),
        details.StartAtUtc,
        details.EndAtUtc,
        details.LocalDate,
        details.StartTime,
        details.EndTime,
        details.Status,
        details.CustomerNotes,
        details.CancelledAtUtc,
        details.CancellationReason,
        details.CreatedAtUtc);

    private static AppointmentSummaryResponse MapAppointmentSummary(AppointmentDetails details) => new(
        details.Id,
        new AppointmentBusinessResponse(
            details.Business.Id,
            details.Business.Name,
            details.Business.Slug,
            details.Business.TimeZoneId),
        new AppointmentServiceResponse(
            details.Service.Id,
            details.Service.NameSnapshot,
            details.Service.DurationMinutesSnapshot,
            details.Service.PriceAmountSnapshot,
            details.Service.CurrencyCodeSnapshot),
        new AppointmentStaffMemberResponse(
            details.StaffMember.Id,
            details.StaffMember.DisplayName),
        new AppointmentCustomerResponse(
            details.Customer.Id,
            details.Customer.FirstName,
            details.Customer.LastName,
            details.Customer.Email),
        details.StartAtUtc,
        details.EndAtUtc,
        details.LocalDate,
        details.StartTime,
        details.EndTime,
        details.Status,
        details.CustomerNotes,
        details.InternalNotes,
        details.CancelledAtUtc,
        details.CancellationReason,
        details.CreatedAtUtc);
}
