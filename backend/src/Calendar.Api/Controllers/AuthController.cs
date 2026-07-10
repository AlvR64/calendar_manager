using Calendar.Api.Contracts.Auth;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.RegisterBusiness;
using Calendar.Application.Auth.RegisterCustomer;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    ICommandHandler<RegisterBusinessCommand, RegisterBusinessResult> registerBusinessHandler,
    ICommandHandler<RegisterCustomerCommand, RegisterCustomerResult> registerCustomerHandler) : ControllerBase
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

    [HttpPost("register-customer")]
    [ProducesResponseType<RegisterCustomerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterCustomerResponse>> RegisterCustomer(
        RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber);

        var result = await registerCustomerHandler.HandleAsync(command, cancellationToken);

        if (!result.Succeeded)
        {
            return Conflict(CreateConflictProblemDetails(result.Error));
        }

        var response = new RegisterCustomerResponse(
            result.CustomerId!.Value,
            result.Email!,
            result.FirstName!,
            result.LastName,
            result.PhoneNumber,
            result.CreatedAtUtc!.Value);

        return Created($"/api/customers/{response.CustomerId}", response);
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

    private ProblemDetails CreateConflictProblemDetails(RegisterCustomerError error) => error switch
    {
        RegisterCustomerError.CustomerEmailAlreadyExists => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Customer email already exists.",
            Detail = "A customer with the provided email already exists.",
            Instance = HttpContext.Request.Path
        },
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Registration conflict.",
            Detail = "The customer could not be registered because of a conflict.",
            Instance = HttpContext.Request.Path
        }
    };
}
