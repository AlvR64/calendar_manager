namespace Calendar.Application.StaffMembers.CreateStaffMember;

public sealed record CreateStaffMemberResult(
    bool Succeeded,
    CreateStaffMemberError Error,
    Guid? StaffMemberId,
    Guid? BusinessId,
    string? DisplayName,
    string? Email,
    string? PhoneNumber,
    string? Bio,
    bool? IsActive,
    int? SortOrder,
    DateTimeOffset? CreatedAtUtc)
{
    public static CreateStaffMemberResult Success(
        Guid staffMemberId,
        Guid businessId,
        string displayName,
        string? email,
        string? phoneNumber,
        string? bio,
        bool isActive,
        int sortOrder,
        DateTimeOffset createdAtUtc) => new(
            true,
            CreateStaffMemberError.None,
            staffMemberId,
            businessId,
            displayName,
            email,
            phoneNumber,
            bio,
            isActive,
            sortOrder,
            createdAtUtc);

    public static CreateStaffMemberResult Failure(CreateStaffMemberError error) => new(
        false,
        error,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null);
}
