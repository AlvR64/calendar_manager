using System.Security.Claims;
using Calendar.Api.Contracts.Appointments;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.CancelCustomerAppointment;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Application.Appointments.ListCustomerAppointments;
using Calendar.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/customers/current/appointments")]
public sealed class CustomerAppointmentsController(
    IQueryHandler<ListCustomerAppointmentsQuery, IReadOnlyList<AppointmentDetails>> listCustomerAppointmentsHandler,
    ICommandHandler<CancelCustomerAppointmentCommand, CancelCustomerAppointmentResult> cancelCustomerAppointmentHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AppointmentSummaryResponse[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AppointmentSummaryResponse>>> ListAppointments(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        if (!TryParseStatus(status, out var appointmentStatus))
        {
            return BadRequest(CreateInvalidStatusProblemDetails());
        }

        var appointments = await listCustomerAppointmentsHandler.HandleAsync(
            new ListCustomerAppointmentsQuery(customerId, from, to, appointmentStatus),
            cancellationToken);

        return Ok(appointments.Select(MapAppointmentSummary).ToList());
    }

    [HttpPost("{appointmentId:guid}/cancel")]
    [ProducesResponseType<AppointmentDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDetailsResponse>> CancelAppointment(
        Guid appointmentId,
        CancelAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        var result = await cancelCustomerAppointmentHandler.HandleAsync(
            new CancelCustomerAppointmentCommand(customerId, appointmentId, request.CancellationReason),
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToActionResult(result.Error);
        }

        return Ok(MapAppointmentDetails(result.Appointment!));
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var customerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(customerIdValue, out customerId);
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

    private ActionResult ToActionResult(CancelCustomerAppointmentError error) => error switch
    {
        CancelCustomerAppointmentError.AppointmentNotFound => NotFound(CreateAppointmentNotFoundProblemDetails()),
        CancelCustomerAppointmentError.Forbidden => Forbid(),
        CancelCustomerAppointmentError.AlreadyCancelled => Conflict(CreateAppointmentAlreadyCancelledProblemDetails()),
        CancelCustomerAppointmentError.AppointmentNotCancelable => Conflict(CreateAppointmentNotCancelableProblemDetails()),
        CancelCustomerAppointmentError.AppointmentInPast => Conflict(CreatePastAppointmentProblemDetails()),
        _ => BadRequest()
    };

    private ProblemDetails CreateInvalidStatusProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid appointment status.",
        Detail = "The status filter must be a valid appointment status.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAppointmentNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Appointment not found.",
        Detail = "The appointment was not found.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAppointmentAlreadyCancelledProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Appointment already cancelled.",
        Detail = "The appointment is already cancelled.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateAppointmentNotCancelableProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Appointment cannot be cancelled.",
        Detail = "Only scheduled appointments can be cancelled by the customer.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreatePastAppointmentProblemDetails() => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Past appointment cannot be cancelled.",
        Detail = "Only future appointments can be cancelled by the customer.",
        Instance = HttpContext.Request.Path
    };

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
}
