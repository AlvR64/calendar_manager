using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMembers.UpdateStaffMemberActiveState;

public sealed class UpdateStaffMemberActiveStateCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateStaffMemberActiveStateCommand, UpdateStaffMemberActiveStateResult>
{
    public async Task<UpdateStaffMemberActiveStateResult> HandleAsync(
        UpdateStaffMemberActiveStateCommand command,
        CancellationToken cancellationToken)
    {
        var staffMember = await staffMemberRepository.GetByIdAndBusinessIdForUpdateAsync(
            command.StaffMemberId,
            command.BusinessId,
            cancellationToken);

        if (staffMember is null)
        {
            return UpdateStaffMemberActiveStateResult.Failure(UpdateStaffMemberActiveStateError.StaffMemberNotFound);
        }

        staffMember.IsActive = command.IsActive;
        staffMember.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateStaffMemberActiveStateResult.Success(MapStaffMember(staffMember));
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
