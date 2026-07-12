using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Businesses.UpdateBusinessBookingWindow;

public sealed class UpdateBusinessBookingWindowCommandHandler(
    IBusinessRepository businessRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateBusinessBookingWindowCommand, UpdateBusinessBookingWindowResult>
{
    public async Task<UpdateBusinessBookingWindowResult> HandleAsync(
        UpdateBusinessBookingWindowCommand command,
        CancellationToken cancellationToken)
    {
        var business = await businessRepository.GetByIdAsync(command.BusinessId, cancellationToken);
        if (business is null)
        {
            return UpdateBusinessBookingWindowResult.Failure(UpdateBusinessBookingWindowError.BusinessNotFound);
        }

        business.MaxAdvanceBookingDays = command.MaxAdvanceBookingDays;
        business.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateBusinessBookingWindowResult.Success(business.MaxAdvanceBookingDays);
    }
}
