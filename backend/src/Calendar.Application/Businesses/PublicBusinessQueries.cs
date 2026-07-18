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

public sealed record SearchPublicBusinessesQuery(
    string? Query,
    string? City,
    string? Category,
    string? Service,
    int Page,
    int PageSize) : IQuery<PublicBusinessSearchResult>;

public sealed class PublicBusinessSearchQueryHandler(IPublicBusinessSearchRepository searchRepository)
    : IQueryHandler<SearchPublicBusinessesQuery, PublicBusinessSearchResult>
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 50;

    public async Task<PublicBusinessSearchResult> HandleAsync(SearchPublicBusinessesQuery query, CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > MaxPageSize)
        {
            return PublicBusinessSearchResult.Failure(PublicBusinessSearchError.InvalidPagination);
        }

        if ((long)(query.Page - 1) * query.PageSize > int.MaxValue)
        {
            return PublicBusinessSearchResult.Failure(PublicBusinessSearchError.InvalidPagination);
        }

        var searchTerm = NormalizeFilter(query.Query);
        var city = NormalizeFilter(query.City);
        var category = NormalizeFilter(query.Category);
        var service = NormalizeFilter(query.Service);

        if (IsInvalidFilter(searchTerm, 200)
            || IsInvalidFilter(city, 100)
            || IsInvalidFilter(category, 80)
            || IsInvalidFilter(service, 200))
        {
            return PublicBusinessSearchResult.Failure(PublicBusinessSearchError.InvalidFilter);
        }

        var page = await searchRepository.SearchAsync(
            new PublicBusinessSearchCriteria(searchTerm, city, category, service, query.Page, query.PageSize),
            cancellationToken);

        return PublicBusinessSearchResult.Success(page);
    }

    private static string? NormalizeFilter(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool IsInvalidFilter(string? value, int maxLength) => value is not null && value.Length > maxLength;
}

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
        business.Category,
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
