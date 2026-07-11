using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Services.CreateService;

public sealed record CreateServiceCommand(
    Guid BusinessId,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal PriceAmount,
    int SortOrder) : ICommand<CreateServiceResult>;
