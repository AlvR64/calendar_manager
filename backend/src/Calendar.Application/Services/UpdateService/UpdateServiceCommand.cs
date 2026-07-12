using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Services.UpdateService;

public sealed record UpdateServiceCommand(
    Guid BusinessId,
    Guid ServiceId,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal PriceAmount,
    int SortOrder) : ICommand<UpdateServiceResult>;
