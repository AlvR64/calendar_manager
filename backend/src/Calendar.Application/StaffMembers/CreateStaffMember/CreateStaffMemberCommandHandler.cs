using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMembers.CreateStaffMember;

public sealed class CreateStaffMemberCommandHandler(
    IBusinessRepository businessRepository,
    IStaffMemberRepository staffMemberRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateStaffMemberCommand, CreateStaffMemberResult>
{
    public async Task<CreateStaffMemberResult> HandleAsync(
        CreateStaffMemberCommand command,
        CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsByIdAsync(command.BusinessId, cancellationToken))
        {
            return CreateStaffMemberResult.Failure(CreateStaffMemberError.BusinessNotFound);
        }

        var now = DateTimeOffset.UtcNow;
        var staffMemberId = Guid.NewGuid();
        var displayName = command.DisplayName.Trim();
        var email = NormalizeOptionalText(command.Email);
        var phoneNumber = NormalizeOptionalText(command.PhoneNumber);
        var bio = NormalizeOptionalText(command.Bio);

        var staffMember = new StaffMember
        {
            Id = staffMemberId,
            BusinessId = command.BusinessId,
            DisplayName = displayName,
            Email = email,
            PhoneNumber = phoneNumber,
            Bio = bio,
            IsActive = true,
            SortOrder = command.SortOrder,
            CreatedAtUtc = now
        };

        staffMemberRepository.Add(staffMember);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateStaffMemberResult.Success(
            staffMemberId,
            command.BusinessId,
            displayName,
            email,
            phoneNumber,
            bio,
            true,
            command.SortOrder,
            now);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
