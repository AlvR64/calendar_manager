using Calendar.Application.Abstractions.Messaging;

namespace Calendar.Application.Businesses.UpdateBusinessBookingWindow;

public sealed record UpdateBusinessBookingWindowCommand(
    Guid BusinessId,
    int MaxAdvanceBookingDays) : ICommand<UpdateBusinessBookingWindowResult>;
