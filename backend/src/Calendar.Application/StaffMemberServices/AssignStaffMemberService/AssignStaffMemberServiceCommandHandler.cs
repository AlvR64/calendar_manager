using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMemberServices.AssignStaffMemberService;

public sealed class AssignStaffMemberServiceCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IServiceRepository serviceRepository,
    IStaffMemberServiceRepository staffMemberServiceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AssignStaffMemberServiceCommand, AssignStaffMemberServiceResult>
{
    public async Task<AssignStaffMemberServiceResult> HandleAsync(
        AssignStaffMemberServiceCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(
                command.StaffMemberId,
                command.BusinessId,
                cancellationToken))
        {
            return AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.StaffMemberNotFound);
        }

        if (!await serviceRepository.ExistsByIdAndBusinessIdAsync(
                command.ServiceId,
                command.BusinessId,
                cancellationToken))
        {
            return AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.ServiceNotFound);
        }

        if (await staffMemberServiceRepository.ExistsAsync(
                command.StaffMemberId,
                command.ServiceId,
                cancellationToken))
        {
            return AssignStaffMemberServiceResult.Failure(AssignStaffMemberServiceError.AssignmentAlreadyExists);
        }

        var now = DateTimeOffset.UtcNow;
        var assignment = new StaffMemberService
        {
            StaffMemberId = command.StaffMemberId,
            ServiceId = command.ServiceId,
            IsActive = true,
            CreatedAtUtc = now
        };

        staffMemberServiceRepository.Add(assignment);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AssignStaffMemberServiceResult.Success(
            command.StaffMemberId,
            command.ServiceId,
            true,
            now);
    }
}
