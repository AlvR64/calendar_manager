using System.Security.Claims;
using Calendar.Api.Contracts.Appointments;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.CreateAppointment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class AppointmentsControllerTests
{
    [Fact]
    public async Task CreateAppointment_WhenRequestIsValid_ReturnsCreatedResponse()
    {
        var customerId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var startAtUtc = DateTimeOffset.UtcNow;
        var endAtUtc = startAtUtc.AddMinutes(30);
        var createdAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCreateAppointmentHandler(CreateAppointmentResult.Success(
            appointmentId,
            businessId,
            staffMemberId,
            serviceId,
            customerId,
            startAtUtc,
            endAtUtc,
            "Scheduled",
            "Notas",
            "Corte",
            30,
            18m,
            "EUR",
            createdAtUtc));
        var controller = CreateController(handler, customerId.ToString());

        var result = await controller.CreateAppointment(CreateRequest(businessId, serviceId, staffMemberId, startAtUtc), CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/api/appointments/{appointmentId}");

        var response = createdResult.Value.Should().BeOfType<AppointmentResponse>().Subject;
        response.Id.Should().Be(appointmentId);
        response.BusinessId.Should().Be(businessId);
        response.StaffMemberId.Should().Be(staffMemberId);
        response.ServiceId.Should().Be(serviceId);
        response.CustomerId.Should().Be(customerId);
        response.StartAtUtc.Should().Be(startAtUtc);
        response.EndAtUtc.Should().Be(endAtUtc);
        response.Status.Should().Be("Scheduled");
        response.CustomerNotes.Should().Be("Notas");
        response.ServiceNameSnapshot.Should().Be("Corte");
        response.ServiceDurationMinutesSnapshot.Should().Be(30);
        response.PriceAmountSnapshot.Should().Be(18m);
        response.CurrencyCodeSnapshot.Should().Be("EUR");
        response.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public async Task CreateAppointment_UsesCustomerIdFromToken()
    {
        var customerId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var startAtUtc = DateTimeOffset.UtcNow;
        var handler = new StubCreateAppointmentHandler(CreateSuccessResult(customerId, businessId, serviceId, staffMemberId, startAtUtc));
        var controller = CreateController(handler, customerId.ToString());
        using var cancellationTokenSource = new CancellationTokenSource();

        await controller.CreateAppointment(CreateRequest(businessId, serviceId, staffMemberId, startAtUtc), cancellationTokenSource.Token);

        handler.Command.Should().NotBeNull();
        handler.Command!.CustomerId.Should().Be(customerId);
        handler.Command.BusinessId.Should().Be(businessId);
        handler.Command.ServiceId.Should().Be(serviceId);
        handler.Command.StaffMemberId.Should().Be(staffMemberId);
        handler.Command.StartAtUtc.Should().Be(startAtUtc);
        handler.Command.CustomerNotes.Should().Be("Notas");
        handler.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task CreateAppointment_WhenCustomerClaimIsMissing_ReturnsForbid()
    {
        var handler = new StubCreateAppointmentHandler(CreateSuccessResult(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
        var controller = CreateController(handler, customerIdClaimValue: null);

        var result = await controller.CreateAppointment(CreateRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow), CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
        handler.Command.Should().BeNull();
    }

    [Fact]
    public async Task CreateAppointment_WhenServiceDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubCreateAppointmentHandler(CreateAppointmentResult.Failure(CreateAppointmentError.ServiceNotFound));
        var controller = CreateController(handler, Guid.NewGuid().ToString());

        var result = await controller.CreateAppointment(CreateRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow), CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Service not found.");
        problemDetails.Instance.Should().Be("/api/appointments");
    }

    [Fact]
    public async Task CreateAppointment_WhenSlotOverlaps_ReturnsConflictProblemDetails()
    {
        var handler = new StubCreateAppointmentHandler(CreateAppointmentResult.Failure(CreateAppointmentError.AppointmentOverlaps));
        var controller = CreateController(handler, Guid.NewGuid().ToString());

        var result = await controller.CreateAppointment(CreateRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow), CancellationToken.None);

        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var problemDetails = conflictResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Appointment slot unavailable.");
        problemDetails.Instance.Should().Be("/api/appointments");
    }

    private static CreateAppointmentRequest CreateRequest(Guid businessId, Guid serviceId, Guid staffMemberId, DateTimeOffset startAtUtc) => new()
    {
        BusinessId = businessId,
        ServiceId = serviceId,
        StaffMemberId = staffMemberId,
        StartAtUtc = startAtUtc,
        CustomerNotes = "Notas"
    };

    private static CreateAppointmentResult CreateSuccessResult(Guid customerId, Guid businessId, Guid serviceId, Guid staffMemberId, DateTimeOffset startAtUtc) =>
        CreateAppointmentResult.Success(
            Guid.NewGuid(),
            businessId,
            staffMemberId,
            serviceId,
            customerId,
            startAtUtc,
            startAtUtc.AddMinutes(30),
            "Scheduled",
            "Notas",
            "Corte",
            30,
            18m,
            "EUR",
            DateTimeOffset.UtcNow);

    private static AppointmentsController CreateController(StubCreateAppointmentHandler handler, string? customerIdClaimValue)
    {
        var controller = new AppointmentsController(handler)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var claims = new List<Claim>();
        if (customerIdClaimValue is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, customerIdClaimValue));
        }

        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        controller.ControllerContext.HttpContext.Request.Path = "/api/appointments";

        return controller;
    }

    private sealed class StubCreateAppointmentHandler(CreateAppointmentResult result) : ICommandHandler<CreateAppointmentCommand, CreateAppointmentResult>
    {
        public CreateAppointmentCommand? Command { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<CreateAppointmentResult> HandleAsync(CreateAppointmentCommand command, CancellationToken cancellationToken)
        {
            Command = command;
            CancellationToken = cancellationToken;
            return Task.FromResult(result);
        }
    }
}
