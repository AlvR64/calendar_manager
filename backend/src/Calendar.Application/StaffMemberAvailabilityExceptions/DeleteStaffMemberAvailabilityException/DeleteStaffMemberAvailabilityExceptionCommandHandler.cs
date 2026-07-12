using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.DeleteStaffMemberAvailabilityException;

public sealed class DeleteStaffMemberAvailabilityExceptionCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberAvailabilityExceptionRepository availabilityExceptionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteStaffMemberAvailabilityExceptionCommand, DeleteStaffMemberAvailabilityExceptionResult>
{
    public async Task<DeleteStaffMemberAvailabilityExceptionResult> HandleAsync(
        DeleteStaffMemberAvailabilityExceptionCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(command.StaffMemberId, command.BusinessId, cancellationToken))
        {
            return DeleteStaffMemberAvailabilityExceptionResult.Failure(DeleteStaffMemberAvailabilityExceptionError.StaffMemberNotFound);
        }

        var exception = await availabilityExceptionRepository.GetByIdAndStaffMemberIdForUpdateAsync(
            command.ExceptionId,
            command.StaffMemberId,
            cancellationToken);

        if (exception is null)
        {
            return DeleteStaffMemberAvailabilityExceptionResult.Failure(DeleteStaffMemberAvailabilityExceptionError.AvailabilityExceptionNotFound);
        }

        availabilityExceptionRepository.Remove(exception);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeleteStaffMemberAvailabilityExceptionResult.Success();
    }
}
