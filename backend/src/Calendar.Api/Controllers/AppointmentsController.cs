using System.Security.Claims;
using Calendar.Api.Contracts.Appointments;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.CreateAppointment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController(
    ICommandHandler<CreateAppointmentCommand, CreateAppointmentResult> createAppointmentHandler) : ControllerBase
{
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

    private ProblemDetails CreateBusinessNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Business not found.",
        Detail = "The business was not found or is not active.",
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
}
