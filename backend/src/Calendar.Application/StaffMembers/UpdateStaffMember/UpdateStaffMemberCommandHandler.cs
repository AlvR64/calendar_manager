using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMembers.UpdateStaffMember;

public sealed class UpdateStaffMemberCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberResult>
{
    public async Task<UpdateStaffMemberResult> HandleAsync(
        UpdateStaffMemberCommand command,
        CancellationToken cancellationToken)
    {
        var staffMember = await staffMemberRepository.GetByIdAndBusinessIdForUpdateAsync(
            command.StaffMemberId,
            command.BusinessId,
            cancellationToken);

        if (staffMember is null)
        {
            return UpdateStaffMemberResult.Failure(UpdateStaffMemberError.StaffMemberNotFound);
        }

        staffMember.DisplayName = command.DisplayName.Trim();
        staffMember.Email = NormalizeOptionalText(command.Email);
        staffMember.PhoneNumber = NormalizeOptionalText(command.PhoneNumber);
        staffMember.Bio = NormalizeOptionalText(command.Bio);
        staffMember.SortOrder = command.SortOrder;
        staffMember.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateStaffMemberResult.Success(MapStaffMember(staffMember));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
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
