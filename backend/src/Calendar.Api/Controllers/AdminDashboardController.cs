using System.Security.Claims;
using Calendar.Api.Contracts.AdminDashboard;
using Calendar.Api.Contracts.Appointments;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.AdminDashboard.GetAdminDashboardSummary;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard-summary")]
public sealed class AdminDashboardController(
    IQueryHandler<GetAdminDashboardSummaryQuery, GetAdminDashboardSummaryResult> getDashboardSummaryHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AdminDashboardSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminDashboardSummaryResponse>> GetDashboardSummary(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var result = await getDashboardSummaryHandler.HandleAsync(
            new GetAdminDashboardSummaryQuery(businessId, from, to),
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToActionResult(result.Error!.Value);
        }

        return Ok(MapSummary(result.Summary!));
    }

    private bool TryGetBusinessId(out Guid businessId)
    {
        var businessIdValue = User.FindFirstValue("business_id");
        return Guid.TryParse(businessIdValue, out businessId);
    }

    private ActionResult ToActionResult(GetAdminDashboardSummaryError error) => error switch
    {
        GetAdminDashboardSummaryError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
        GetAdminDashboardSummaryError.InvalidBusinessTimeZone => BadRequest(CreateInvalidBusinessTimeZoneProblemDetails()),
        GetAdminDashboardSummaryError.InvalidDateRange => BadRequest(CreateInvalidDateRangeProblemDetails()),
        GetAdminDashboardSummaryError.DateRangeTooLarge => BadRequest(CreateDateRangeTooLargeProblemDetails()),
        _ => BadRequest()
    };

    private ProblemDetails CreateBusinessNotFoundProblemDetails() => new()
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Business not found.",
        Detail = "The business associated with the current admin account was not found.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidBusinessTimeZoneProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid business time zone.",
        Detail = "The business time zone id must be a valid IANA time zone id.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateInvalidDateRangeProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid dashboard date range.",
        Detail = "The to date must be on or after the from date.",
        Instance = HttpContext.Request.Path
    };

    private ProblemDetails CreateDateRangeTooLargeProblemDetails() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Dashboard date range is too large.",
        Detail = "The dashboard date range must be 90 days or fewer.",
        Instance = HttpContext.Request.Path
    };

    private static AdminDashboardSummaryResponse MapSummary(AdminDashboardSummary summary) => new(
        summary.TodayAppointmentCount,
        summary.UpcomingAppointments.Select(MapAppointmentSummary).ToList(),
        summary.EstimatedRevenueAmount,
        summary.CurrencyCode,
        summary.StatusCounts.Select(count => new AdminDashboardStatusCountResponse(count.Status, count.Count)).ToList(),
        summary.RangeStartLocalDate,
        summary.RangeEndLocalDate);

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
