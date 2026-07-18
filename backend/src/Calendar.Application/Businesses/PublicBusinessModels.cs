namespace Calendar.Application.Businesses;

public sealed record BusinessDetails(
    Guid Id,
    string Name,
    string Slug,
    string? Category,
    string? Description,
    string? ContactEmail,
    string? ContactPhoneNumber,
    string? WebsiteUrl,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? PostalCode,
    string? CountryCode,
    string TimeZoneId,
    string CurrencyCode,
    int MaxAdvanceBookingDays);

public sealed record BusinessServiceDetails(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal PriceAmount,
    int SortOrder);

public sealed record BusinessStaffMemberDetails(
    Guid Id,
    string DisplayName,
    string? Bio,
    int SortOrder);

public sealed record BusinessStaffMemberServiceAssignmentDetails(
    Guid StaffMemberId,
    Guid ServiceId);

public sealed record BusinessProfileDetails(
    BusinessDetails Business,
    IReadOnlyList<BusinessServiceDetails> Services,
    IReadOnlyList<BusinessStaffMemberDetails> StaffMembers,
    IReadOnlyList<BusinessStaffMemberServiceAssignmentDetails> Assignments);

public sealed record ListBusinessServicesResult(
    bool BusinessFound,
    IReadOnlyList<BusinessServiceDetails> Services)
{
    public static ListBusinessServicesResult Success(IReadOnlyList<BusinessServiceDetails> services) => new(true, services);

    public static ListBusinessServicesResult NotFound() => new(false, []);
}

public sealed record ListBusinessStaffMembersResult(
    bool BusinessFound,
    IReadOnlyList<BusinessStaffMemberDetails> StaffMembers)
{
    public static ListBusinessStaffMembersResult Success(IReadOnlyList<BusinessStaffMemberDetails> staffMembers) => new(true, staffMembers);

    public static ListBusinessStaffMembersResult NotFound() => new(false, []);
}

public sealed record PublicBusinessFeaturedServiceDetails(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal PriceAmount);

public sealed record PublicBusinessCardDetails(
    Guid Id,
    string Slug,
    string Name,
    string? Description,
    string? City,
    string? CountryCode,
    string? Category,
    string TimeZoneId,
    string CurrencyCode,
    IReadOnlyList<PublicBusinessFeaturedServiceDetails> FeaturedServices,
    decimal? StartingPriceAmount);

public sealed record PublicBusinessSearchPage(
    IReadOnlyList<PublicBusinessCardDetails> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage);

public enum PublicBusinessSearchError
{
    InvalidPagination,
    InvalidFilter
}

public sealed record PublicBusinessSearchResult(
    bool Succeeded,
    PublicBusinessSearchPage? Page,
    PublicBusinessSearchError? Error)
{
    public static PublicBusinessSearchResult Success(PublicBusinessSearchPage page) => new(true, page, null);

    public static PublicBusinessSearchResult Failure(PublicBusinessSearchError error) => new(false, null, error);
}
