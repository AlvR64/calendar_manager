using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Services;

public sealed record ListAdminServicesQuery(Guid BusinessId) : IQuery<ListAdminServicesResult>;

public sealed record GetAdminServiceQuery(Guid BusinessId, Guid ServiceId) : IQuery<GetAdminServiceResult>;

public sealed class AdminServiceQueryHandler(
    IBusinessRepository businessRepository,
    IServiceRepository serviceRepository)
    : IQueryHandler<ListAdminServicesQuery, ListAdminServicesResult>,
        IQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>
{
    public async Task<ListAdminServicesResult> HandleAsync(
        ListAdminServicesQuery query,
        CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsByIdAsync(query.BusinessId, cancellationToken))
        {
            return ListAdminServicesResult.NotFound();
        }

        var services = await serviceRepository.ListByBusinessIdAsync(query.BusinessId, cancellationToken);
        return ListAdminServicesResult.Success(services.Select(MapService).ToList());
    }

    public async Task<GetAdminServiceResult> HandleAsync(
        GetAdminServiceQuery query,
        CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsByIdAsync(query.BusinessId, cancellationToken))
        {
            return GetAdminServiceResult.Failure(GetAdminServiceError.BusinessNotFound);
        }

        var service = await serviceRepository.GetByIdAndBusinessIdAsync(
            query.ServiceId,
            query.BusinessId,
            cancellationToken);

        return service is null
            ? GetAdminServiceResult.Failure(GetAdminServiceError.ServiceNotFound)
            : GetAdminServiceResult.Success(MapService(service));
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
