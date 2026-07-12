using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Services.DeleteService;

public sealed record DeleteServiceCommand(Guid BusinessId, Guid ServiceId) : ICommand<DeleteServiceResult>;
