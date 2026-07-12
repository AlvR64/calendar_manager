using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.StaffMemberServices.UpdateStaffMemberServiceActiveState;

public sealed class UpdateStaffMemberServiceActiveStateCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IServiceRepository serviceRepository,
    IStaffMemberServiceRepository staffMemberServiceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateStaffMemberServiceActiveStateCommand, UpdateStaffMemberServiceActiveStateResult>
{
    public async Task<UpdateStaffMemberServiceActiveStateResult> HandleAsync(
        UpdateStaffMemberServiceActiveStateCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(
                command.StaffMemberId,
                command.BusinessId,
                cancellationToken))
        {
            return UpdateStaffMemberServiceActiveStateResult.Failure(UpdateStaffMemberServiceActiveStateError.StaffMemberNotFound);
        }

        if (!await serviceRepository.ExistsByIdAndBusinessIdAsync(
                command.ServiceId,
                command.BusinessId,
                cancellationToken))
        {
            return UpdateStaffMemberServiceActiveStateResult.Failure(UpdateStaffMemberServiceActiveStateError.ServiceNotFound);
        }

        var assignment = await staffMemberServiceRepository.GetByIdsForUpdateAsync(
            command.StaffMemberId,
            command.ServiceId,
            cancellationToken);

        if (assignment is null)
        {
            return UpdateStaffMemberServiceActiveStateResult.Failure(UpdateStaffMemberServiceActiveStateError.AssignmentNotFound);
        }

        assignment.IsActive = command.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateStaffMemberServiceActiveStateResult.Success(
            assignment.StaffMemberId,
            assignment.ServiceId,
            assignment.IsActive,
            assignment.CreatedAtUtc);
    }
}
