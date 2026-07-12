namespace Calendar.Application.StaffMembers;

public sealed record AdminStaffMemberDetails(
    Guid Id,
    Guid BusinessId,
    string DisplayName,
    string? Email,
    string? PhoneNumber,
    string? Bio,
    bool IsActive,
    int SortOrder,
    DateTimeOffset CreatedAtUtc);

public sealed record ListAdminStaffMembersResult(
    bool BusinessFound,
    IReadOnlyList<AdminStaffMemberDetails> StaffMembers)
{
    public static ListAdminStaffMembersResult Success(IReadOnlyList<AdminStaffMemberDetails> staffMembers) => new(true, staffMembers);

    public static ListAdminStaffMembersResult NotFound() => new(false, []);
}

public enum GetAdminStaffMemberError
{
    None = 0,
    BusinessNotFound = 1,
    StaffMemberNotFound = 2
}

public sealed record GetAdminStaffMemberResult(
    bool Succeeded,
    GetAdminStaffMemberError Error,
    AdminStaffMemberDetails? StaffMember)
{
    public static GetAdminStaffMemberResult Success(AdminStaffMemberDetails staffMember) => new(
        true,
        GetAdminStaffMemberError.None,
        staffMember);

    public static GetAdminStaffMemberResult Failure(GetAdminStaffMemberError error) => new(
        false,
        error,
        null);
}
