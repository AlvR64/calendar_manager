using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilityExceptions;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.UpdateStaffMemberAvailabilityException;

public sealed class UpdateStaffMemberAvailabilityExceptionCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberAvailabilityExceptionRepository availabilityExceptionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateStaffMemberAvailabilityExceptionCommand, UpdateStaffMemberAvailabilityExceptionResult>
{
    private const int MaxReasonLength = 250;

    public async Task<UpdateStaffMemberAvailabilityExceptionResult> HandleAsync(
        UpdateStaffMemberAvailabilityExceptionCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(command.StaffMemberId, command.BusinessId, cancellationToken))
        {
            return UpdateStaffMemberAvailabilityExceptionResult.Failure(UpdateStaffMemberAvailabilityExceptionError.StaffMemberNotFound);
        }

        var exception = await availabilityExceptionRepository.GetByIdAndStaffMemberIdForUpdateAsync(
            command.ExceptionId,
            command.StaffMemberId,
            cancellationToken);

        if (exception is null)
        {
            return UpdateStaffMemberAvailabilityExceptionResult.Failure(UpdateStaffMemberAvailabilityExceptionError.AvailabilityExceptionNotFound);
        }

        if (command.Reason?.Length > MaxReasonLength)
        {
            return UpdateStaffMemberAvailabilityExceptionResult.Failure(UpdateStaffMemberAvailabilityExceptionError.ReasonTooLong);
        }

        if (command.IsClosed)
        {
            if (command.StartTime.HasValue || command.EndTime.HasValue)
            {
                return UpdateStaffMemberAvailabilityExceptionResult.Failure(UpdateStaffMemberAvailabilityExceptionError.InvalidClosedException);
            }

            if (await availabilityExceptionRepository.HasAnyExceptionForDateAsync(command.StaffMemberId, command.LocalDate, command.ExceptionId, cancellationToken))
            {
                return UpdateStaffMemberAvailabilityExceptionResult.Failure(UpdateStaffMemberAvailabilityExceptionError.AvailabilityExceptionAlreadyExists);
            }
        }
        else
        {
            if (!command.StartTime.HasValue || !command.EndTime.HasValue || command.StartTime.Value >= command.EndTime.Value)
            {
                return UpdateStaffMemberAvailabilityExceptionResult.Failure(UpdateStaffMemberAvailabilityExceptionError.InvalidTimeRange);
            }

            if (await availabilityExceptionRepository.HasClosedExceptionAsync(command.StaffMemberId, command.LocalDate, command.ExceptionId, cancellationToken))
            {
                return UpdateStaffMemberAvailabilityExceptionResult.Failure(UpdateStaffMemberAvailabilityExceptionError.AvailabilityExceptionAlreadyExists);
            }

            if (await availabilityExceptionRepository.OverlapsAsync(
                    command.StaffMemberId,
                    command.LocalDate,
                    command.StartTime.Value,
                    command.EndTime.Value,
                    command.ExceptionId,
                    cancellationToken))
            {
                return UpdateStaffMemberAvailabilityExceptionResult.Failure(UpdateStaffMemberAvailabilityExceptionError.AvailabilityExceptionOverlaps);
            }
        }

        exception.LocalDate = command.LocalDate;
        exception.IsClosed = command.IsClosed;
        exception.StartTime = command.IsClosed ? null : command.StartTime;
        exception.EndTime = command.IsClosed ? null : command.EndTime;
        exception.Reason = command.Reason;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateStaffMemberAvailabilityExceptionResult.Success(MapException(exception));
    }

    private static StaffMemberAvailabilityExceptionDetails MapException(StaffMemberAvailabilityException exception) => new(
        exception.Id,
        exception.StaffMemberId,
        exception.LocalDate,
        exception.IsClosed,
        exception.StartTime,
        exception.EndTime,
        exception.Reason,
        exception.CreatedAtUtc);
}
