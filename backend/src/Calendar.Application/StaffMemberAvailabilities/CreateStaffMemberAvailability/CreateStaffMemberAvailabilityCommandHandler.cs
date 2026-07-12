using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilities;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMemberAvailabilities.CreateStaffMemberAvailability;

public sealed class CreateStaffMemberAvailabilityCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberAvailabilityRepository availabilityRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateStaffMemberAvailabilityCommand, CreateStaffMemberAvailabilityResult>
{
    public async Task<CreateStaffMemberAvailabilityResult> HandleAsync(
        CreateStaffMemberAvailabilityCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(command.StaffMemberId, command.BusinessId, cancellationToken))
        {
            return CreateStaffMemberAvailabilityResult.Failure(CreateStaffMemberAvailabilityError.StaffMemberNotFound);
        }

        if (!TryGetDayOfWeek(command.DayOfWeek, out var dayOfWeek))
        {
            return CreateStaffMemberAvailabilityResult.Failure(CreateStaffMemberAvailabilityError.InvalidDayOfWeek);
        }

        if (command.StartTime >= command.EndTime)
        {
            return CreateStaffMemberAvailabilityResult.Failure(CreateStaffMemberAvailabilityError.InvalidTimeRange);
        }

        if (await availabilityRepository.OverlapsAsync(
                command.StaffMemberId,
                dayOfWeek,
                command.StartTime,
                command.EndTime,
                excludedAvailabilityId: null,
                cancellationToken))
        {
            return CreateStaffMemberAvailabilityResult.Failure(CreateStaffMemberAvailabilityError.AvailabilityOverlaps);
        }

        var availability = new StaffMemberAvailability
        {
            Id = Guid.NewGuid(),
            StaffMemberId = command.StaffMemberId,
            DayOfWeek = dayOfWeek,
            StartTime = command.StartTime,
            EndTime = command.EndTime,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        availabilityRepository.Add(availability);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateStaffMemberAvailabilityResult.Success(MapAvailability(availability));
    }

    private static bool TryGetDayOfWeek(int value, out DayOfWeek dayOfWeek)
    {
        if (value is < 0 or > 6)
        {
            dayOfWeek = default;
            return false;
        }

        dayOfWeek = (DayOfWeek)value;
        return true;
    }

    private static StaffMemberAvailabilityDetails MapAvailability(StaffMemberAvailability availability) => new(
        availability.Id,
        availability.StaffMemberId,
        (int)availability.DayOfWeek,
        availability.StartTime,
        availability.EndTime,
        availability.IsActive,
        availability.CreatedAtUtc);
}
