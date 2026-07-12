using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.StaffMemberAvailabilities.DeleteStaffMemberAvailability;

public sealed class DeleteStaffMemberAvailabilityCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberAvailabilityRepository availabilityRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteStaffMemberAvailabilityCommand, DeleteStaffMemberAvailabilityResult>
{
    public async Task<DeleteStaffMemberAvailabilityResult> HandleAsync(
        DeleteStaffMemberAvailabilityCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(command.StaffMemberId, command.BusinessId, cancellationToken))
        {
            return DeleteStaffMemberAvailabilityResult.Failure(DeleteStaffMemberAvailabilityError.StaffMemberNotFound);
        }

        var availability = await availabilityRepository.GetByIdAndStaffMemberIdForUpdateAsync(
            command.AvailabilityId,
            command.StaffMemberId,
            cancellationToken);

        if (availability is null)
        {
            return DeleteStaffMemberAvailabilityResult.Failure(DeleteStaffMemberAvailabilityError.AvailabilityNotFound);
        }

        availabilityRepository.Remove(availability);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeleteStaffMemberAvailabilityResult.Success();
    }
}
