using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilityExceptions;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.CreateStaffMemberAvailabilityException;

public sealed class CreateStaffMemberAvailabilityExceptionCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberAvailabilityExceptionRepository availabilityExceptionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateStaffMemberAvailabilityExceptionCommand, CreateStaffMemberAvailabilityExceptionResult>
{
    private const int MaxReasonLength = 250;

    public async Task<CreateStaffMemberAvailabilityExceptionResult> HandleAsync(
        CreateStaffMemberAvailabilityExceptionCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(command.StaffMemberId, command.BusinessId, cancellationToken))
        {
            return CreateStaffMemberAvailabilityExceptionResult.Failure(CreateStaffMemberAvailabilityExceptionError.StaffMemberNotFound);
        }

        if (command.Reason?.Length > MaxReasonLength)
        {
            return CreateStaffMemberAvailabilityExceptionResult.Failure(CreateStaffMemberAvailabilityExceptionError.ReasonTooLong);
        }

        if (command.IsClosed)
        {
            if (command.StartTime.HasValue || command.EndTime.HasValue)
            {
                return CreateStaffMemberAvailabilityExceptionResult.Failure(CreateStaffMemberAvailabilityExceptionError.InvalidClosedException);
            }

            if (await availabilityExceptionRepository.HasAnyExceptionForDateAsync(command.StaffMemberId, command.LocalDate, cancellationToken))
            {
                return CreateStaffMemberAvailabilityExceptionResult.Failure(CreateStaffMemberAvailabilityExceptionError.AvailabilityExceptionAlreadyExists);
            }
        }
        else
        {
            if (!command.StartTime.HasValue || !command.EndTime.HasValue || command.StartTime.Value >= command.EndTime.Value)
            {
                return CreateStaffMemberAvailabilityExceptionResult.Failure(CreateStaffMemberAvailabilityExceptionError.InvalidTimeRange);
            }

            if (await availabilityExceptionRepository.HasClosedExceptionAsync(command.StaffMemberId, command.LocalDate, cancellationToken))
            {
                return CreateStaffMemberAvailabilityExceptionResult.Failure(CreateStaffMemberAvailabilityExceptionError.AvailabilityExceptionAlreadyExists);
            }

            if (await availabilityExceptionRepository.OverlapsAsync(
                    command.StaffMemberId,
                    command.LocalDate,
                    command.StartTime.Value,
                    command.EndTime.Value,
                    cancellationToken))
            {
                return CreateStaffMemberAvailabilityExceptionResult.Failure(CreateStaffMemberAvailabilityExceptionError.AvailabilityExceptionOverlaps);
            }
        }

        var exception = new StaffMemberAvailabilityException
        {
            Id = Guid.NewGuid(),
            StaffMemberId = command.StaffMemberId,
            LocalDate = command.LocalDate,
            IsClosed = command.IsClosed,
            StartTime = command.IsClosed ? null : command.StartTime,
            EndTime = command.IsClosed ? null : command.EndTime,
            Reason = command.Reason,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        availabilityExceptionRepository.Add(exception);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateStaffMemberAvailabilityExceptionResult.Success(MapException(exception));
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
