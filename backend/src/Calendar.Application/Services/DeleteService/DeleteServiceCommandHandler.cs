using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Services.DeleteService;

public sealed class DeleteServiceCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteServiceCommand, DeleteServiceResult>
{
    public async Task<DeleteServiceResult> HandleAsync(DeleteServiceCommand command, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAndBusinessIdForUpdateAsync(
            command.ServiceId,
            command.BusinessId,
            cancellationToken);

        if (service is null)
        {
            return DeleteServiceResult.Failure(DeleteServiceError.ServiceNotFound);
        }

        if (await serviceRepository.HasAppointmentsAsync(command.ServiceId, cancellationToken))
        {
            return DeleteServiceResult.Failure(DeleteServiceError.ServiceHasAppointments);
        }

        serviceRepository.Remove(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DeleteServiceResult.Success();
    }
}
