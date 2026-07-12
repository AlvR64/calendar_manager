using Calendar.Api.Contracts.Businesses;
using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Businesses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/businesses")]
public sealed class BusinessesController(
    IQueryHandler<GetBusinessByIdQuery, BusinessDetails?> getBusinessByIdHandler,
    IQueryHandler<GetBusinessProfileByIdQuery, BusinessProfileDetails?> getBusinessProfileByIdHandler,
    IQueryHandler<GetBusinessProfileBySlugQuery, BusinessProfileDetails?> getBusinessProfileBySlugHandler,
    IQueryHandler<ListBusinessServicesQuery, ListBusinessServicesResult> listBusinessServicesHandler,
    IQueryHandler<GetBusinessServiceQuery, BusinessServiceDetails?> getBusinessServiceHandler,
    IQueryHandler<ListBusinessStaffMembersQuery, ListBusinessStaffMembersResult> listBusinessStaffMembersHandler,
    IQueryHandler<GetBusinessStaffMemberQuery, BusinessStaffMemberDetails?> getBusinessStaffMemberHandler) : ControllerBase
{
    [HttpGet("{businessId:guid}")]
    [ProducesResponseType<BusinessResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessResponse>> GetBusinessById(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var business = await getBusinessByIdHandler.HandleAsync(new GetBusinessByIdQuery(businessId), cancellationToken);
        return business is null ? NotFound(CreateBusinessNotFoundProblemDetails()) : Ok(MapBusiness(business));
    }

    [HttpGet("{businessId:guid}/profile")]
    [ProducesResponseType<BusinessProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessProfileResponse>> GetBusinessProfileById(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var profile = await getBusinessProfileByIdHandler.HandleAsync(new GetBusinessProfileByIdQuery(businessId), cancellationToken);
        return profile is null ? NotFound(CreateBusinessNotFoundProblemDetails()) : Ok(MapProfile(profile));
    }

    [HttpGet("by-slug/{slug}/profile")]
    [ProducesResponseType<BusinessProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessProfileResponse>> GetBusinessProfileBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var profile = await getBusinessProfileBySlugHandler.HandleAsync(new GetBusinessProfileBySlugQuery(slug), cancellationToken);
        return profile is null ? NotFound(CreateBusinessNotFoundProblemDetails()) : Ok(MapProfile(profile));
    }

    [HttpGet("{businessId:guid}/services")]
    [ProducesResponseType<IReadOnlyList<BusinessServiceResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BusinessServiceResponse>>> ListBusinessServices(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var result = await listBusinessServicesHandler.HandleAsync(new ListBusinessServicesQuery(businessId), cancellationToken);
        return result.BusinessFound
            ? Ok(result.Services.Select(MapService).ToList())
            : NotFound(CreateBusinessNotFoundProblemDetails());
    }

    [HttpGet("{businessId:guid}/services/{serviceId:guid}")]
    [ProducesResponseType<BusinessServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessServiceResponse>> GetBusinessService(
        Guid businessId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var service = await getBusinessServiceHandler.HandleAsync(new GetBusinessServiceQuery(businessId, serviceId), cancellationToken);
        return service is null ? NotFound(CreateServiceNotFoundProblemDetails()) : Ok(MapService(service));
    }

    [HttpGet("{businessId:guid}/staff-members")]
    [ProducesResponseType<IReadOnlyList<BusinessStaffMemberResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BusinessStaffMemberResponse>>> ListBusinessStaffMembers(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        var result = await listBusinessStaffMembersHandler.HandleAsync(new ListBusinessStaffMembersQuery(businessId), cancellationToken);
        return result.BusinessFound
            ? Ok(result.StaffMembers.Select(MapStaffMember).ToList())
            : NotFound(CreateBusinessNotFoundProblemDetails());
    }

    [HttpGet("{businessId:guid}/staff-members/{staffMemberId:guid}")]
    [ProducesResponseType<BusinessStaffMemberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessStaffMemberResponse>> GetBusinessStaffMember(
        Guid businessId,
        Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        var staffMember = await getBusinessStaffMemberHandler.HandleAsync(
            new GetBusinessStaffMemberQuery(businessId, staffMemberId),
            cancellationToken);

        return staffMember is null ? NotFound(CreateStaffMemberNotFoundProblemDetails()) : Ok(MapStaffMember(staffMember));
    }

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

    private static BusinessProfileResponse MapProfile(BusinessProfileDetails profile) => new(
        MapBusiness(profile.Business),
        profile.Services.Select(MapService).ToList(),
        profile.StaffMembers.Select(MapStaffMember).ToList(),
        profile.Assignments.Select(MapAssignment).ToList());

    private static BusinessResponse MapBusiness(BusinessDetails business) => new(
        business.Id,
        business.Name,
        business.Slug,
        business.Description,
        business.ContactEmail,
        business.ContactPhoneNumber,
        business.WebsiteUrl,
        business.AddressLine1,
        business.AddressLine2,
        business.City,
        business.PostalCode,
        business.CountryCode,
        business.TimeZoneId,
        business.CurrencyCode,
        business.MaxAdvanceBookingDays);

    private static BusinessServiceResponse MapService(BusinessServiceDetails service) => new(
        service.Id,
        service.Name,
        service.Description,
        service.DurationMinutes,
        service.PriceAmount,
        service.SortOrder);

    private static BusinessStaffMemberResponse MapStaffMember(BusinessStaffMemberDetails staffMember) => new(
        staffMember.Id,
        staffMember.DisplayName,
        staffMember.Bio,
        staffMember.SortOrder);

    private static BusinessStaffMemberServiceAssignmentResponse MapAssignment(
        BusinessStaffMemberServiceAssignmentDetails assignment) => new(
            assignment.StaffMemberId,
            assignment.ServiceId);
}
