using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Services.CreateService;

public sealed class CreateServiceCommandHandler(
    IBusinessRepository businessRepository,
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateServiceCommand, CreateServiceResult>
{
    public async Task<CreateServiceResult> HandleAsync(
        CreateServiceCommand command,
        CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsByIdAsync(command.BusinessId, cancellationToken))
        {
            return CreateServiceResult.Failure(CreateServiceError.BusinessNotFound);
        }

        var now = DateTimeOffset.UtcNow;
        var serviceId = Guid.NewGuid();
        var name = command.Name.Trim();
        var description = NormalizeOptionalText(command.Description);

        var service = new Service
        {
            Id = serviceId,
            BusinessId = command.BusinessId,
            Name = name,
            Description = description,
            DurationMinutes = command.DurationMinutes,
            PriceAmount = command.PriceAmount,
            IsActive = true,
            SortOrder = command.SortOrder,
            CreatedAtUtc = now
        };

        serviceRepository.Add(service);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateServiceResult.Success(
            serviceId,
            command.BusinessId,
            name,
            description,
            command.DurationMinutes,
            command.PriceAmount,
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
