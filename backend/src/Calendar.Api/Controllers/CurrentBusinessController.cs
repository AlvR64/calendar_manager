using System.Security.Claims;
using Calendar.Api.Contracts.Businesses;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Businesses.UpdateBusinessBookingWindow;
using Calendar.Application.Businesses.UpdateBusinessDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/businesses/current")]
public sealed class CurrentBusinessController(
    ICommandHandler<UpdateBusinessDetailsCommand, UpdateBusinessDetailsResult> updateBusinessDetailsHandler,
    ICommandHandler<UpdateBusinessBookingWindowCommand, UpdateBusinessBookingWindowResult> updateBusinessBookingWindowHandler) : ControllerBase
{
    [HttpPut]
    [ProducesResponseType<BusinessResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessResponse>> UpdateBusinessDetails(
        UpdateBusinessDetailsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new UpdateBusinessDetailsCommand(
            businessId,
            request.Name,
            request.Description,
            request.ContactEmail,
            request.ContactPhoneNumber,
            request.WebsiteUrl,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.PostalCode,
            request.CountryCode,
            request.TimeZoneId,
            request.CurrencyCode);

        var result = await updateBusinessDetailsHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UpdateBusinessDetailsError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        var response = new BusinessResponse(
            result.BusinessId!.Value,
            result.Name!,
            result.Slug!,
            result.Description,
            result.ContactEmail,
            result.ContactPhoneNumber,
            result.WebsiteUrl,
            result.AddressLine1,
            result.AddressLine2,
            result.City,
            result.PostalCode,
            result.CountryCode,
            result.TimeZoneId!,
            result.CurrencyCode!,
            result.MaxAdvanceBookingDays!.Value);

        return Ok(response);
    }

    [HttpPut("booking-window")]
    [ProducesResponseType<BusinessBookingWindowResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessBookingWindowResponse>> UpdateBusinessBookingWindow(
        UpdateBusinessBookingWindowRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }

        var command = new UpdateBusinessBookingWindowCommand(businessId, request.MaxAdvanceBookingDays);

        var result = await updateBusinessBookingWindowHandler.HandleAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error switch
            {
                UpdateBusinessBookingWindowError.BusinessNotFound => NotFound(CreateBusinessNotFoundProblemDetails()),
                _ => BadRequest()
            };
        }

        return Ok(new BusinessBookingWindowResponse(result.MaxAdvanceBookingDays!.Value));
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
