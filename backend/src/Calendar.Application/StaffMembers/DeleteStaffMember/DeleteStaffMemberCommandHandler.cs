using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.StaffMembers.DeleteStaffMember;

public sealed class DeleteStaffMemberCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteStaffMemberCommand, DeleteStaffMemberResult>
{
    public async Task<DeleteStaffMemberResult> HandleAsync(
        DeleteStaffMemberCommand command,
        CancellationToken cancellationToken)
    {
        var staffMember = await staffMemberRepository.GetByIdAndBusinessIdForUpdateAsync(
            command.StaffMemberId,
            command.BusinessId,
            cancellationToken);

        if (staffMember is null)
        {
            return DeleteStaffMemberResult.Failure(DeleteStaffMemberError.StaffMemberNotFound);
        }

        if (await staffMemberRepository.HasAppointmentsAsync(command.StaffMemberId, cancellationToken))
        {
            return DeleteStaffMemberResult.Failure(DeleteStaffMemberError.StaffMemberHasAppointments);
        }

        staffMemberRepository.Remove(staffMember);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeleteStaffMemberResult.Success();
    }
}
