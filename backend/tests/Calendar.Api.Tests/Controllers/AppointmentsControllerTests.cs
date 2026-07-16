using System.Security.Claims;
using Calendar.Api.Contracts.Appointments;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.CreateAppointment;
using Calendar.Application.Appointments.GetAppointmentDetails;
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

    [Fact]
    public async Task GetAppointment_WhenCustomerOwnsAppointment_ReturnsDetails()
    {
        var customerId = Guid.NewGuid();
        var details = CreateDetails(customerId: customerId);
        var queryHandler = new StubGetAppointmentDetailsHandler(details);
        var controller = CreateController(CreateDefaultCreateHandler(), queryHandler, customerId.ToString(), role: "Customer", businessIdClaimValue: null);

        var result = await controller.GetAppointment(details.Id, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AppointmentDetailsResponse>().Subject;
        response.Id.Should().Be(details.Id);
        response.Business.Id.Should().Be(details.Business.Id);
        response.Business.Name.Should().Be(details.Business.Name);
        response.Business.Slug.Should().Be(details.Business.Slug);
        response.Business.TimeZoneId.Should().Be(details.Business.TimeZoneId);
        response.Service.Id.Should().Be(details.Service.Id);
        response.Service.NameSnapshot.Should().Be(details.Service.NameSnapshot);
        response.StaffMember.DisplayName.Should().Be(details.StaffMember.DisplayName);
        response.Customer.Id.Should().Be(customerId);
        response.LocalDate.Should().Be(details.LocalDate);
        response.StartTime.Should().Be(details.StartTime);
        response.EndTime.Should().Be(details.EndTime);
        response.Status.Should().Be("Scheduled");
        response.CustomerNotes.Should().Be("Notas");
    }

    [Fact]
    public async Task GetAppointment_WhenAdminBusinessMatches_ReturnsDetails()
    {
        var businessId = Guid.NewGuid();
        var details = CreateDetails(businessId: businessId);
        var queryHandler = new StubGetAppointmentDetailsHandler(details);
        var controller = CreateController(CreateDefaultCreateHandler(), queryHandler, Guid.NewGuid().ToString(), role: "Admin", businessId.ToString());

        var result = await controller.GetAppointment(details.Id, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
        queryHandler.Query.Should().Be(new GetAppointmentDetailsQuery(details.Id));
    }

    [Fact]
    public async Task GetAppointment_WhenAppointmentDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var queryHandler = new StubGetAppointmentDetailsHandler(null);
        var controller = CreateController(CreateDefaultCreateHandler(), queryHandler, Guid.NewGuid().ToString(), role: "Customer", businessIdClaimValue: null);

        var result = await controller.GetAppointment(Guid.NewGuid(), CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Appointment not found.");
        problemDetails.Instance.Should().Be("/api/appointments/appointment-1");
    }

    [Fact]
    public async Task GetAppointment_WhenCustomerDoesNotOwnAppointment_ReturnsForbid()
    {
        var details = CreateDetails(customerId: Guid.NewGuid());
        var queryHandler = new StubGetAppointmentDetailsHandler(details);
        var controller = CreateController(CreateDefaultCreateHandler(), queryHandler, Guid.NewGuid().ToString(), role: "Customer", businessIdClaimValue: null);

        var result = await controller.GetAppointment(details.Id, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetAppointment_WhenAdminBusinessDoesNotMatch_ReturnsForbid()
    {
        var details = CreateDetails(businessId: Guid.NewGuid());
        var queryHandler = new StubGetAppointmentDetailsHandler(details);
        var controller = CreateController(CreateDefaultCreateHandler(), queryHandler, Guid.NewGuid().ToString(), role: "Admin", businessIdClaimValue: Guid.NewGuid().ToString());

        var result = await controller.GetAppointment(details.Id, CancellationToken.None);

        result.Result.Should().BeOfType<ForbidResult>();
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

    private static AppointmentDetails CreateDetails(Guid? customerId = null, Guid? businessId = null)
    {
        var startAtUtc = new DateTimeOffset(2026, 7, 20, 8, 0, 0, TimeSpan.Zero);
        var endAtUtc = startAtUtc.AddMinutes(30);

        return new AppointmentDetails(
            Guid.NewGuid(),
            new AppointmentBusinessDetails(businessId ?? Guid.NewGuid(), "Barberia Centro", "barberia-centro", "Europe/Madrid"),
            new AppointmentServiceDetails(Guid.NewGuid(), "Corte", 30, 18m, "EUR"),
            new AppointmentStaffMemberDetails(Guid.NewGuid(), "Ana"),
            new AppointmentCustomerDetails(customerId ?? Guid.NewGuid(), "Clara", "Diaz", "clara@example.test"),
            startAtUtc,
            endAtUtc,
            new DateOnly(2026, 7, 20),
            new TimeOnly(10, 0),
            new TimeOnly(10, 30),
            "Scheduled",
            "Notas",
            null,
            null,
            DateTimeOffset.UtcNow);
    }

    private static StubCreateAppointmentHandler CreateDefaultCreateHandler() =>
        new(CreateSuccessResult(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));

    private static AppointmentsController CreateController(StubCreateAppointmentHandler handler, string? customerIdClaimValue)
    {
        var controller = CreateController(handler, new StubGetAppointmentDetailsHandler(null), customerIdClaimValue, role: "Customer", businessIdClaimValue: null);
        controller.ControllerContext.HttpContext.Request.Path = "/api/appointments";

        return controller;
    }

    private static AppointmentsController CreateController(
        StubCreateAppointmentHandler createHandler,
        StubGetAppointmentDetailsHandler getHandler,
        string? customerIdClaimValue,
        string role,
        string? businessIdClaimValue)
    {
        var controller = new AppointmentsController(createHandler, getHandler)
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

        claims.Add(new Claim(ClaimTypes.Role, role));
        if (businessIdClaimValue is not null)
        {
            claims.Add(new Claim("business_id", businessIdClaimValue));
        }

        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        controller.ControllerContext.HttpContext.Request.Path = "/api/appointments/appointment-1";

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

    private sealed class StubGetAppointmentDetailsHandler(AppointmentDetails? result) : IQueryHandler<GetAppointmentDetailsQuery, AppointmentDetails?>
    {
        public GetAppointmentDetailsQuery? Query { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<AppointmentDetails?> HandleAsync(GetAppointmentDetailsQuery query, CancellationToken cancellationToken)
        {
            Query = query;
            CancellationToken = cancellationToken;
            return Task.FromResult(result);
        }
    }
}
