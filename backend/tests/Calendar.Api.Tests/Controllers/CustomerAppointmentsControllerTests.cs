using System.Security.Claims;
using Calendar.Api.Contracts.Appointments;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.CancelCustomerAppointment;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Application.Appointments.ListCustomerAppointments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class CustomerAppointmentsControllerTests
{
    [Fact]
    public async Task ListAppointments_WhenRequestIsValid_ReturnsCustomerAppointmentSummaries()
    {
        var customerId = Guid.NewGuid();
        var details = CreateDetails(customerId);
        var listHandler = new StubListCustomerAppointmentsHandler([details]);
        var controller = CreateController(customerId.ToString(), listHandler, new StubCancelCustomerAppointmentHandler(CreateCancelSuccess(details)));

        var result = await controller.ListAppointments(null, null, "Scheduled", CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<IReadOnlyList<AppointmentSummaryResponse>>().Subject;
        response.Should().ContainSingle();
        response[0].Id.Should().Be(details.Id);
        response[0].Business.Id.Should().Be(details.Business.Id);
        response[0].Service.NameSnapshot.Should().Be(details.Service.NameSnapshot);
        response[0].StaffMember.DisplayName.Should().Be(details.StaffMember.DisplayName);
        response[0].LocalDate.Should().Be(details.LocalDate);
        response[0].Status.Should().Be("Scheduled");
        listHandler.Query.Should().NotBeNull();
        listHandler.Query!.CustomerId.Should().Be(customerId);
        listHandler.Query.Status.Should().Be(Calendar.Domain.Entities.AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task ListAppointments_WhenStatusIsInvalid_ReturnsBadRequest()
    {
        var listHandler = new StubListCustomerAppointmentsHandler([]);
        var controller = CreateController(Guid.NewGuid().ToString(), listHandler, new StubCancelCustomerAppointmentHandler(CreateCancelSuccess(CreateDetails(Guid.NewGuid()))));

        var result = await controller.ListAppointments(null, null, "invalid", CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problemDetails = badRequest.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be("Invalid appointment status.");
        listHandler.Query.Should().BeNull();
    }

    [Fact]
    public async Task ListAppointments_WhenCustomerClaimIsMissing_ReturnsForbid()
    {
        var listHandler = new StubListCustomerAppointmentsHandler([]);
        var controller = CreateController(customerIdClaimValue: null, listHandler, new StubCancelCustomerAppointmentHandler(CreateCancelSuccess(CreateDetails(Guid.NewGuid()))));

        var result = await controller.ListAppointments(null, null, null, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        listHandler.Query.Should().BeNull();
    }

    [Fact]
    public async Task CancelAppointment_WhenRequestIsValid_ReturnsCancelledAppointmentDetails()
    {
        var customerId = Guid.NewGuid();
        var details = CreateDetails(customerId, status: "CancelledByCustomer", cancelledAtUtc: DateTimeOffset.UtcNow, cancellationReason: "No puedo ir");
        var cancelHandler = new StubCancelCustomerAppointmentHandler(CreateCancelSuccess(details));
        var controller = CreateController(customerId.ToString(), new StubListCustomerAppointmentsHandler([]), cancelHandler);

        var result = await controller.CancelAppointment(details.Id, new CancelAppointmentRequest { CancellationReason = "No puedo ir" }, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AppointmentDetailsResponse>().Subject;
        response.Id.Should().Be(details.Id);
        response.Status.Should().Be("CancelledByCustomer");
        response.CancellationReason.Should().Be("No puedo ir");
        cancelHandler.Command.Should().NotBeNull();
        cancelHandler.Command!.CustomerId.Should().Be(customerId);
        cancelHandler.Command.AppointmentId.Should().Be(details.Id);
        cancelHandler.Command.CancellationReason.Should().Be("No puedo ir");
    }

    [Fact]
    public async Task CancelAppointment_WhenHandlerReturnsForbidden_ReturnsForbid()
    {
        var cancelHandler = new StubCancelCustomerAppointmentHandler(CancelCustomerAppointmentResult.Failure(CancelCustomerAppointmentError.Forbidden));
        var controller = CreateController(Guid.NewGuid().ToString(), new StubListCustomerAppointmentsHandler([]), cancelHandler);

        var result = await controller.CancelAppointment(Guid.NewGuid(), new CancelAppointmentRequest(), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CancelAppointment_WhenAppointmentIsAlreadyCancelled_ReturnsConflict()
    {
        var cancelHandler = new StubCancelCustomerAppointmentHandler(CancelCustomerAppointmentResult.Failure(CancelCustomerAppointmentError.AlreadyCancelled));
        var controller = CreateController(Guid.NewGuid().ToString(), new StubListCustomerAppointmentsHandler([]), cancelHandler);

        var result = await controller.CancelAppointment(Guid.NewGuid(), new CancelAppointmentRequest(), CancellationToken.None);

        var conflict = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problemDetails = conflict.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Appointment already cancelled.");
    }

    private static CustomerAppointmentsController CreateController(
        string? customerIdClaimValue,
        StubListCustomerAppointmentsHandler listHandler,
        StubCancelCustomerAppointmentHandler cancelHandler)
    {
        var controller = new CustomerAppointmentsController(listHandler, cancelHandler)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var claims = new List<Claim> { new(ClaimTypes.Role, "Customer") };
        if (customerIdClaimValue is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, customerIdClaimValue));
        }

        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        controller.ControllerContext.HttpContext.Request.Path = "/api/customers/current/appointments";

        return controller;
    }

    private static AppointmentDetails CreateDetails(
        Guid customerId,
        string status = "Scheduled",
        DateTimeOffset? cancelledAtUtc = null,
        string? cancellationReason = null)
    {
        var startAtUtc = new DateTimeOffset(2026, 7, 20, 8, 0, 0, TimeSpan.Zero);

        return new AppointmentDetails(
            Guid.NewGuid(),
            new AppointmentBusinessDetails(Guid.NewGuid(), "Barberia Centro", "barberia-centro", "Europe/Madrid"),
            new AppointmentServiceDetails(Guid.NewGuid(), "Corte", 30, 18m, "EUR"),
            new AppointmentStaffMemberDetails(Guid.NewGuid(), "Ana"),
            new AppointmentCustomerDetails(customerId, "Clara", "Diaz", "clara@example.test"),
            startAtUtc,
            startAtUtc.AddMinutes(30),
            new DateOnly(2026, 7, 20),
            new TimeOnly(10, 0),
            new TimeOnly(10, 30),
            status,
            "Notas",
            cancelledAtUtc,
            cancellationReason,
            DateTimeOffset.UtcNow);
    }

    private static CancelCustomerAppointmentResult CreateCancelSuccess(AppointmentDetails details) =>
        CancelCustomerAppointmentResult.Success(details);

    private sealed class StubListCustomerAppointmentsHandler(IReadOnlyList<AppointmentDetails> result) : IQueryHandler<ListCustomerAppointmentsQuery, IReadOnlyList<AppointmentDetails>>
    {
        public ListCustomerAppointmentsQuery? Query { get; private set; }

        public Task<IReadOnlyList<AppointmentDetails>> HandleAsync(ListCustomerAppointmentsQuery query, CancellationToken cancellationToken)
        {
            Query = query;
            return Task.FromResult(result);
        }
    }

    private sealed class StubCancelCustomerAppointmentHandler(CancelCustomerAppointmentResult result) : ICommandHandler<CancelCustomerAppointmentCommand, CancelCustomerAppointmentResult>
    {
        public CancelCustomerAppointmentCommand? Command { get; private set; }

        public Task<CancelCustomerAppointmentResult> HandleAsync(CancelCustomerAppointmentCommand command, CancellationToken cancellationToken)
        {
            Command = command;
            return Task.FromResult(result);
        }
    }
}
