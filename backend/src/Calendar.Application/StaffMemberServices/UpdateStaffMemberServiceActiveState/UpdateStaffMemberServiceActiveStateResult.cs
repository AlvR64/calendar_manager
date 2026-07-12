namespace Calendar.Application.StaffMemberServices.UpdateStaffMemberServiceActiveState;

public sealed record UpdateStaffMemberServiceActiveStateResult(
    bool Succeeded,
    UpdateStaffMemberServiceActiveStateError Error,
    Guid? StaffMemberId = null,
    Guid? ServiceId = null,
    bool? IsActive = null,
    DateTimeOffset? CreatedAtUtc = null)
{
    public static UpdateStaffMemberServiceActiveStateResult Success(
        Guid staffMemberId,
        Guid serviceId,
        bool isActive,
        DateTimeOffset createdAtUtc) => new(
            true,
            UpdateStaffMemberServiceActiveStateError.None,
            staffMemberId,
            serviceId,
            isActive,
            createdAtUtc);

    public static UpdateStaffMemberServiceActiveStateResult Failure(UpdateStaffMemberServiceActiveStateError error) => new(
        false,
        error);
}
