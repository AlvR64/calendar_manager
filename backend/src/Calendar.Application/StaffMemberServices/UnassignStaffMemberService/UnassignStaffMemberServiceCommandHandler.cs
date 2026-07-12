using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.StaffMemberServices.UnassignStaffMemberService;

public sealed class UnassignStaffMemberServiceCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IServiceRepository serviceRepository,
    IStaffMemberServiceRepository staffMemberServiceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UnassignStaffMemberServiceCommand, UnassignStaffMemberServiceResult>
{
    public async Task<UnassignStaffMemberServiceResult> HandleAsync(
        UnassignStaffMemberServiceCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(
                command.StaffMemberId,
                command.BusinessId,
                cancellationToken))
        {
            return UnassignStaffMemberServiceResult.Failure(UnassignStaffMemberServiceError.StaffMemberNotFound);
        }

        if (!await serviceRepository.ExistsByIdAndBusinessIdAsync(
                command.ServiceId,
                command.BusinessId,
                cancellationToken))
        {
            return UnassignStaffMemberServiceResult.Failure(UnassignStaffMemberServiceError.ServiceNotFound);
        }

        var assignment = await staffMemberServiceRepository.GetByIdsForUpdateAsync(
            command.StaffMemberId,
            command.ServiceId,
            cancellationToken);

        if (assignment is null)
        {
            return UnassignStaffMemberServiceResult.Failure(UnassignStaffMemberServiceError.AssignmentNotFound);
        }

        if (await staffMemberServiceRepository.HasAppointmentsAsync(
                command.StaffMemberId,
                command.ServiceId,
                cancellationToken))
        {
            return UnassignStaffMemberServiceResult.Failure(UnassignStaffMemberServiceError.AssignmentHasAppointments);
        }

        staffMemberServiceRepository.Remove(assignment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnassignStaffMemberServiceResult.Success();
    }
}
