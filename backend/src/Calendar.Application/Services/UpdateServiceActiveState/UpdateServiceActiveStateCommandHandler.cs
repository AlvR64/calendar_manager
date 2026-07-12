using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Services.UpdateServiceActiveState;

public sealed class UpdateServiceActiveStateCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateServiceActiveStateCommand, UpdateServiceActiveStateResult>
{
    public async Task<UpdateServiceActiveStateResult> HandleAsync(
        UpdateServiceActiveStateCommand command,
        CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAndBusinessIdForUpdateAsync(
            command.ServiceId,
            command.BusinessId,
            cancellationToken);

        if (service is null)
        {
            return UpdateServiceActiveStateResult.Failure(UpdateServiceActiveStateError.ServiceNotFound);
        }

        service.IsActive = command.IsActive;
        service.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateServiceActiveStateResult.Success(MapService(service));
    }

    private static AdminServiceDetails MapService(Service service) => new(
        service.Id,
        service.BusinessId,
        service.Name,
        service.Description,
        service.DurationMinutes,
        service.PriceAmount,
        service.IsActive,
        service.SortOrder,
        service.CreatedAtUtc);
}
