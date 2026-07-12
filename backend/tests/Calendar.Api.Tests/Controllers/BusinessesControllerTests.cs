using Calendar.Api.Contracts.Businesses;
using Calendar.Api.Controllers;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Businesses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Tests.Controllers;

public sealed class BusinessesControllerTests
{
    [Fact]
    public async Task GetBusinessById_WhenBusinessExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetBusinessByIdQuery, BusinessDetails?>(CreateBusinessDetails(businessId));
        var controller = CreateController(getBusinessByIdHandler: handler, requestPath: $"/api/businesses/{businessId}");

        var result = await controller.GetBusinessById(businessId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BusinessResponse>().Subject;
        response.Id.Should().Be(businessId);
        response.Name.Should().Be("Barberia Centro");
        response.Slug.Should().Be("barberia-centro-madrid");
        response.TimeZoneId.Should().Be("Europe/Madrid");
        response.CurrencyCode.Should().Be("EUR");
        response.MaxAdvanceBookingDays.Should().Be(60);
    }

    [Fact]
    public async Task GetBusinessById_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var businessId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetBusinessByIdQuery, BusinessDetails?>(null);
        var controller = CreateController(getBusinessByIdHandler: handler, requestPath: $"/api/businesses/{businessId}");

        var result = await controller.GetBusinessById(businessId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be($"/api/businesses/{businessId}");
    }

    [Fact]
    public async Task GetBusinessProfileById_WhenBusinessExists_ReturnsOkResponse()
    {
        var profile = CreateBusinessProfileDetails();
        var handler = new StubQueryHandler<GetBusinessProfileByIdQuery, BusinessProfileDetails?>(profile);
        var controller = CreateController(getBusinessProfileByIdHandler: handler, requestPath: $"/api/businesses/{profile.Business.Id}/profile");

        var result = await controller.GetBusinessProfileById(profile.Business.Id, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BusinessProfileResponse>().Subject;
        response.Business.Id.Should().Be(profile.Business.Id);
        response.Services.Should().ContainSingle();
        response.StaffMembers.Should().ContainSingle();
        response.Assignments.Should().ContainSingle();
    }

    [Fact]
    public async Task GetBusinessProfileBySlug_WhenBusinessExists_ReturnsOkResponse()
    {
        var profile = CreateBusinessProfileDetails();
        var handler = new StubQueryHandler<GetBusinessProfileBySlugQuery, BusinessProfileDetails?>(profile);
        var controller = CreateController(getBusinessProfileBySlugHandler: handler, requestPath: "/api/businesses/by-slug/barberia-centro-madrid/profile");

        var result = await controller.GetBusinessProfileBySlug("barberia-centro-madrid", CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BusinessProfileResponse>().Subject;
        response.Business.Slug.Should().Be("barberia-centro-madrid");
        handler.Query.Should().Be(new GetBusinessProfileBySlugQuery("barberia-centro-madrid"));
    }

    [Fact]
    public async Task GetBusinessProfileBySlug_WhenBusinessDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var handler = new StubQueryHandler<GetBusinessProfileBySlugQuery, BusinessProfileDetails?>(null);
        var controller = CreateController(getBusinessProfileBySlugHandler: handler, requestPath: "/api/businesses/by-slug/missing/profile");

        var result = await controller.GetBusinessProfileBySlug("missing", CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Business not found.");
        problemDetails.Instance.Should().Be("/api/businesses/by-slug/missing/profile");
    }

    [Fact]
    public async Task ListBusinessServices_WhenBusinessExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var service = CreateServiceDetails();
        var handler = new StubQueryHandler<ListBusinessServicesQuery, ListBusinessServicesResult>(
            ListBusinessServicesResult.Success([service]));
        var controller = CreateController(listBusinessServicesHandler: handler, requestPath: $"/api/businesses/{businessId}/services");

        var result = await controller.ListBusinessServices(businessId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<BusinessServiceResponse>>().Subject;
        response.Should().ContainSingle();
        response[0].Id.Should().Be(service.Id);
    }

    [Fact]
    public async Task GetBusinessService_WhenServiceExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var service = CreateServiceDetails();
        var handler = new StubQueryHandler<GetBusinessServiceQuery, BusinessServiceDetails?>(service);
        var controller = CreateController(getBusinessServiceHandler: handler, requestPath: $"/api/businesses/{businessId}/services/{service.Id}");

        var result = await controller.GetBusinessService(businessId, service.Id, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BusinessServiceResponse>().Subject;
        response.Id.Should().Be(service.Id);
        response.Name.Should().Be("Corte de pelo");
    }

    [Fact]
    public async Task GetBusinessService_WhenServiceDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var businessId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetBusinessServiceQuery, BusinessServiceDetails?>(null);
        var controller = CreateController(getBusinessServiceHandler: handler, requestPath: $"/api/businesses/{businessId}/services/{serviceId}");

        var result = await controller.GetBusinessService(businessId, serviceId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Service not found.");
        problemDetails.Instance.Should().Be($"/api/businesses/{businessId}/services/{serviceId}");
    }

    [Fact]
    public async Task ListBusinessStaffMembers_WhenBusinessExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMember = CreateStaffMemberDetails();
        var handler = new StubQueryHandler<ListBusinessStaffMembersQuery, ListBusinessStaffMembersResult>(
            ListBusinessStaffMembersResult.Success([staffMember]));
        var controller = CreateController(listBusinessStaffMembersHandler: handler, requestPath: $"/api/businesses/{businessId}/staff-members");

        var result = await controller.ListBusinessStaffMembers(businessId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<List<BusinessStaffMemberResponse>>().Subject;
        response.Should().ContainSingle();
        response[0].Id.Should().Be(staffMember.Id);
    }

    [Fact]
    public async Task GetBusinessStaffMember_WhenStaffMemberExists_ReturnsOkResponse()
    {
        var businessId = Guid.NewGuid();
        var staffMember = CreateStaffMemberDetails();
        var handler = new StubQueryHandler<GetBusinessStaffMemberQuery, BusinessStaffMemberDetails?>(staffMember);
        var controller = CreateController(getBusinessStaffMemberHandler: handler, requestPath: $"/api/businesses/{businessId}/staff-members/{staffMember.Id}");

        var result = await controller.GetBusinessStaffMember(businessId, staffMember.Id, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BusinessStaffMemberResponse>().Subject;
        response.Id.Should().Be(staffMember.Id);
        response.DisplayName.Should().Be("Laura Martinez");
        response.Bio.Should().Be("Especialista en cortes y color");
    }

    [Fact]
    public async Task GetBusinessStaffMember_WhenStaffMemberDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        var businessId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();
        var handler = new StubQueryHandler<GetBusinessStaffMemberQuery, BusinessStaffMemberDetails?>(null);
        var controller = CreateController(getBusinessStaffMemberHandler: handler, requestPath: $"/api/businesses/{businessId}/staff-members/{staffMemberId}");

        var result = await controller.GetBusinessStaffMember(businessId, staffMemberId, CancellationToken.None);

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var problemDetails = notFoundResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be("Staff member not found.");
        problemDetails.Instance.Should().Be($"/api/businesses/{businessId}/staff-members/{staffMemberId}");
    }

    private static BusinessesController CreateController(
        StubQueryHandler<GetBusinessByIdQuery, BusinessDetails?>? getBusinessByIdHandler = null,
        StubQueryHandler<GetBusinessProfileByIdQuery, BusinessProfileDetails?>? getBusinessProfileByIdHandler = null,
        StubQueryHandler<GetBusinessProfileBySlugQuery, BusinessProfileDetails?>? getBusinessProfileBySlugHandler = null,
        StubQueryHandler<ListBusinessServicesQuery, ListBusinessServicesResult>? listBusinessServicesHandler = null,
        StubQueryHandler<GetBusinessServiceQuery, BusinessServiceDetails?>? getBusinessServiceHandler = null,
        StubQueryHandler<ListBusinessStaffMembersQuery, ListBusinessStaffMembersResult>? listBusinessStaffMembersHandler = null,
        StubQueryHandler<GetBusinessStaffMemberQuery, BusinessStaffMemberDetails?>? getBusinessStaffMemberHandler = null,
        string requestPath = "/api/businesses") => new(
            getBusinessByIdHandler ?? new StubQueryHandler<GetBusinessByIdQuery, BusinessDetails?>(CreateBusinessDetails(Guid.NewGuid())),
            getBusinessProfileByIdHandler ?? new StubQueryHandler<GetBusinessProfileByIdQuery, BusinessProfileDetails?>(CreateBusinessProfileDetails()),
            getBusinessProfileBySlugHandler ?? new StubQueryHandler<GetBusinessProfileBySlugQuery, BusinessProfileDetails?>(CreateBusinessProfileDetails()),
            listBusinessServicesHandler ?? new StubQueryHandler<ListBusinessServicesQuery, ListBusinessServicesResult>(ListBusinessServicesResult.Success([])),
            getBusinessServiceHandler ?? new StubQueryHandler<GetBusinessServiceQuery, BusinessServiceDetails?>(CreateServiceDetails()),
            listBusinessStaffMembersHandler ?? new StubQueryHandler<ListBusinessStaffMembersQuery, ListBusinessStaffMembersResult>(ListBusinessStaffMembersResult.Success([])),
            getBusinessStaffMemberHandler ?? new StubQueryHandler<GetBusinessStaffMemberQuery, BusinessStaffMemberDetails?>(CreateStaffMemberDetails()))
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Path = requestPath
                }
            }
        }
    };

    private static BusinessProfileDetails CreateBusinessProfileDetails()
    {
        var service = CreateServiceDetails();
        var staffMember = CreateStaffMemberDetails();

        return new BusinessProfileDetails(
            CreateBusinessDetails(Guid.NewGuid()),
            [service],
            [staffMember],
            [new BusinessStaffMemberServiceAssignmentDetails(staffMember.Id, service.Id)]);
    }

    private static BusinessDetails CreateBusinessDetails(Guid businessId) => new(
        businessId,
        "Barberia Centro",
        "barberia-centro-madrid",
        "Barberia de barrio",
        "contacto@barberia-centro.test",
        "+34910000000",
        "https://barberia-centro.test",
        "Calle Mayor 1",
        null,
        "Madrid",
        "28013",
        "ES",
        "Europe/Madrid",
        "EUR",
        60);

    private static BusinessServiceDetails CreateServiceDetails() => new(
        Guid.NewGuid(),
        "Corte de pelo",
        "Corte clasico o moderno",
        30,
        18.00m,
        0);

    private static BusinessStaffMemberDetails CreateStaffMemberDetails() => new(
        Guid.NewGuid(),
        "Laura Martinez",
        "Especialista en cortes y color",
        0);

    private sealed class StubQueryHandler<TQuery, TResult>(TResult result) : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        public TQuery? Query { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            Query = query;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
