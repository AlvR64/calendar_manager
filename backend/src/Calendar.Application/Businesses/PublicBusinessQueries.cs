using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Businesses;

public sealed record GetBusinessByIdQuery(Guid BusinessId) : IQuery<BusinessDetails?>;

public sealed record GetBusinessProfileByIdQuery(Guid BusinessId) : IQuery<BusinessProfileDetails?>;

public sealed record GetBusinessProfileBySlugQuery(string Slug) : IQuery<BusinessProfileDetails?>;

public sealed record ListBusinessServicesQuery(Guid BusinessId) : IQuery<ListBusinessServicesResult>;

public sealed record GetBusinessServiceQuery(Guid BusinessId, Guid ServiceId) : IQuery<BusinessServiceDetails?>;

public sealed record ListBusinessStaffMembersQuery(Guid BusinessId) : IQuery<ListBusinessStaffMembersResult>;

public sealed record GetBusinessStaffMemberQuery(Guid BusinessId, Guid StaffMemberId) : IQuery<BusinessStaffMemberDetails?>;

public sealed class PublicBusinessQueryHandler(
    IBusinessRepository businessRepository,
    IServiceRepository serviceRepository,
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberServiceRepository staffMemberServiceRepository)
    : IQueryHandler<GetBusinessByIdQuery, BusinessDetails?>,
        IQueryHandler<GetBusinessProfileByIdQuery, BusinessProfileDetails?>,
        IQueryHandler<GetBusinessProfileBySlugQuery, BusinessProfileDetails?>,
        IQueryHandler<ListBusinessServicesQuery, ListBusinessServicesResult>,
        IQueryHandler<GetBusinessServiceQuery, BusinessServiceDetails?>,
        IQueryHandler<ListBusinessStaffMembersQuery, ListBusinessStaffMembersResult>,
        IQueryHandler<GetBusinessStaffMemberQuery, BusinessStaffMemberDetails?>
{
    public async Task<BusinessDetails?> HandleAsync(GetBusinessByIdQuery query, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetActiveByIdAsync(query.BusinessId, cancellationToken);
        return business is null ? null : MapBusiness(business);
    }

    public async Task<BusinessProfileDetails?> HandleAsync(GetBusinessProfileByIdQuery query, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetActiveByIdAsync(query.BusinessId, cancellationToken);
        return business is null ? null : await BuildProfileAsync(business, cancellationToken);
    }

    public async Task<BusinessProfileDetails?> HandleAsync(GetBusinessProfileBySlugQuery query, CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetActiveBySlugAsync(query.Slug.Trim().ToLowerInvariant(), cancellationToken);
        return business is null ? null : await BuildProfileAsync(business, cancellationToken);
    }

    public async Task<ListBusinessServicesResult> HandleAsync(ListBusinessServicesQuery query, CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsActiveByIdAsync(query.BusinessId, cancellationToken))
        {
            return ListBusinessServicesResult.NotFound();
        }

        var services = await serviceRepository.ListActiveByBusinessIdAsync(query.BusinessId, cancellationToken);
        return ListBusinessServicesResult.Success(services.Select(MapService).ToList());
    }

    public async Task<BusinessServiceDetails?> HandleAsync(GetBusinessServiceQuery query, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetActiveByIdAndBusinessIdAsync(
            query.ServiceId,
            query.BusinessId,
            cancellationToken);

        return service is null ? null : MapService(service);
    }

    public async Task<ListBusinessStaffMembersResult> HandleAsync(ListBusinessStaffMembersQuery query, CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsActiveByIdAsync(query.BusinessId, cancellationToken))
        {
            return ListBusinessStaffMembersResult.NotFound();
        }

        var staffMembers = await staffMemberRepository.ListActiveByBusinessIdAsync(query.BusinessId, cancellationToken);
        return ListBusinessStaffMembersResult.Success(staffMembers.Select(MapStaffMember).ToList());
    }

    public async Task<BusinessStaffMemberDetails?> HandleAsync(GetBusinessStaffMemberQuery query, CancellationToken cancellationToken)
    {
        var staffMember = await staffMemberRepository.GetActiveByIdAndBusinessIdAsync(
            query.StaffMemberId,
            query.BusinessId,
            cancellationToken);

        return staffMember is null ? null : MapStaffMember(staffMember);
    }

    private async Task<BusinessProfileDetails> BuildProfileAsync(Business business, CancellationToken cancellationToken)
    {
        var services = await serviceRepository.ListActiveByBusinessIdAsync(business.Id, cancellationToken);
        var staffMembers = await staffMemberRepository.ListActiveByBusinessIdAsync(business.Id, cancellationToken);
        var assignments = await staffMemberServiceRepository.ListActiveByBusinessIdAsync(business.Id, cancellationToken);

        return new BusinessProfileDetails(
            MapBusiness(business),
            services.Select(MapService).ToList(),
            staffMembers.Select(MapStaffMember).ToList(),
            assignments.Select(assignment => new BusinessStaffMemberServiceAssignmentDetails(
                assignment.StaffMemberId,
                assignment.ServiceId)).ToList());
    }

    private static BusinessDetails MapBusiness(Business business) => new(
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
        business.CurrencyCode);

    private static BusinessServiceDetails MapService(Service service) => new(
        service.Id,
        service.Name,
        service.Description,
        service.DurationMinutes,
        service.PriceAmount,
        service.SortOrder);

    private static BusinessStaffMemberDetails MapStaffMember(StaffMember staffMember) => new(
        staffMember.Id,
        staffMember.DisplayName,
        staffMember.Bio,
        staffMember.SortOrder);
}
