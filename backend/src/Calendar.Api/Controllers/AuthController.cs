using Calendar.Api.Contracts.Auth;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.RegisterBusiness;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    ICommandHandler<RegisterBusinessCommand, RegisterBusinessResult> registerBusinessHandler) : ControllerBase
{
    [HttpPost("register-business")]
    [ProducesResponseType<RegisterBusinessResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterBusinessResponse>> RegisterBusiness(
        RegisterBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterBusinessCommand(
            request.BusinessName,
            request.BusinessSlug,
            request.TimeZoneId,
            request.CurrencyCode,
            request.AdminEmail,
            request.AdminPassword,
            request.AdminDisplayName);

        var result = await registerBusinessHandler.HandleAsync(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Conflict(CreateConflictProblemDetails(result.Error));
        }

        var response = new RegisterBusinessResponse(
            result.BusinessId!.Value,
            result.BusinessSlug!,
            result.AdminId!.Value,
            result.AdminEmail!,
            result.CreatedAtUtc!.Value);

        return Created($"/api/businesses/{response.BusinessId}", response);
    }

    private ProblemDetails CreateConflictProblemDetails(RegisterBusinessError error) => error switch
    {
        RegisterBusinessError.BusinessSlugAlreadyExists => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Business slug already exists.",
            Detail = "A business with the provided slug already exists.",
            Instance = HttpContext.Request.Path
        },
        RegisterBusinessError.AdminEmailAlreadyExists => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Admin email already exists.",
            Detail = "An admin with the provided email already exists.",
            Instance = HttpContext.Request.Path
        },
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Registration conflict.",
            Detail = "The business could not be registered because of a conflict.",
            Instance = HttpContext.Request.Path
        }
    };
}
