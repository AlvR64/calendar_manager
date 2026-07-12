using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMembers;

public sealed record ListAdminStaffMembersQuery(Guid BusinessId) : IQuery<ListAdminStaffMembersResult>;

public sealed record GetAdminStaffMemberQuery(Guid BusinessId, Guid StaffMemberId) : IQuery<GetAdminStaffMemberResult>;

public sealed class AdminStaffMemberQueryHandler(
    IBusinessRepository businessRepository,
    IStaffMemberRepository staffMemberRepository)
    : IQueryHandler<ListAdminStaffMembersQuery, ListAdminStaffMembersResult>,
        IQueryHandler<GetAdminStaffMemberQuery, GetAdminStaffMemberResult>
{
    public async Task<ListAdminStaffMembersResult> HandleAsync(
        ListAdminStaffMembersQuery query,
        CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsByIdAsync(query.BusinessId, cancellationToken))
        {
            return ListAdminStaffMembersResult.NotFound();
        }

        var staffMembers = await staffMemberRepository.ListByBusinessIdAsync(query.BusinessId, cancellationToken);
        return ListAdminStaffMembersResult.Success(staffMembers.Select(MapStaffMember).ToList());
    }

    public async Task<GetAdminStaffMemberResult> HandleAsync(
        GetAdminStaffMemberQuery query,
        CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsByIdAsync(query.BusinessId, cancellationToken))
        {
            return GetAdminStaffMemberResult.Failure(GetAdminStaffMemberError.BusinessNotFound);
        }

        var staffMember = await staffMemberRepository.GetByIdAndBusinessIdAsync(
            query.StaffMemberId,
            query.BusinessId,
            cancellationToken);

        return staffMember is null
            ? GetAdminStaffMemberResult.Failure(GetAdminStaffMemberError.StaffMemberNotFound)
            : GetAdminStaffMemberResult.Success(MapStaffMember(staffMember));
    }

    private static AdminStaffMemberDetails MapStaffMember(StaffMember staffMember) => new(
        staffMember.Id,
        staffMember.BusinessId,
        staffMember.DisplayName,
        staffMember.Email,
        staffMember.PhoneNumber,
        staffMember.Bio,
        staffMember.IsActive,
        staffMember.SortOrder,
        staffMember.CreatedAtUtc);
}
