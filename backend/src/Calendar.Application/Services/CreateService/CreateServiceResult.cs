namespace Calendar.Application.Services.CreateService;

public sealed record CreateServiceResult(
    bool Succeeded,
    CreateServiceError Error,
    Guid? ServiceId,
    Guid? BusinessId,
    string? Name,
    string? Description,
    int? DurationMinutes,
    decimal? PriceAmount,
    bool? IsActive,
    int? SortOrder,
    DateTimeOffset? CreatedAtUtc)
{
    public static CreateServiceResult Success(
        Guid serviceId,
        Guid businessId,
        string name,
        string? description,
        int durationMinutes,
        decimal priceAmount,
        bool isActive,
        int sortOrder,
        DateTimeOffset createdAtUtc) => new(
            true,
            CreateServiceError.None,
            serviceId,
            businessId,
            name,
            description,
            durationMinutes,
            priceAmount,
            isActive,
            sortOrder,
            createdAtUtc);

    public static CreateServiceResult Failure(CreateServiceError error) => new(
        false,
        error,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null);
}
