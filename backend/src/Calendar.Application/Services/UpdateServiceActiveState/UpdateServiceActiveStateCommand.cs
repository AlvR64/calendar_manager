using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Services.UpdateServiceActiveState;

public sealed record UpdateServiceActiveStateCommand(
    Guid BusinessId,
    Guid ServiceId,
    bool IsActive) : ICommand<UpdateServiceActiveStateResult>;
