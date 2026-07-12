using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Services.UpdateService;

public sealed class UpdateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateServiceCommand, UpdateServiceResult>
{
    public async Task<UpdateServiceResult> HandleAsync(UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAndBusinessIdForUpdateAsync(
            command.ServiceId,
            command.BusinessId,
            cancellationToken);

        if (service is null)
        {
            return UpdateServiceResult.Failure(UpdateServiceError.ServiceNotFound);
        }

        service.Name = command.Name.Trim();
        service.Description = NormalizeOptionalText(command.Description);
        service.DurationMinutes = command.DurationMinutes;
        service.PriceAmount = command.PriceAmount;
        service.SortOrder = command.SortOrder;
        service.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateServiceResult.Success(MapService(service));
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
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
