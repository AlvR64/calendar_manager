using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilities;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMemberAvailabilities.UpdateStaffMemberAvailability;

public sealed class UpdateStaffMemberAvailabilityCommandHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberAvailabilityRepository availabilityRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateStaffMemberAvailabilityCommand, UpdateStaffMemberAvailabilityResult>
{
    public async Task<UpdateStaffMemberAvailabilityResult> HandleAsync(
        UpdateStaffMemberAvailabilityCommand command,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(command.StaffMemberId, command.BusinessId, cancellationToken))
        {
            return UpdateStaffMemberAvailabilityResult.Failure(UpdateStaffMemberAvailabilityError.StaffMemberNotFound);
        }

        var availability = await availabilityRepository.GetByIdAndStaffMemberIdForUpdateAsync(
            command.AvailabilityId,
            command.StaffMemberId,
            cancellationToken);

        if (availability is null)
        {
            return UpdateStaffMemberAvailabilityResult.Failure(UpdateStaffMemberAvailabilityError.AvailabilityNotFound);
        }

        if (!TryGetDayOfWeek(command.DayOfWeek, out var dayOfWeek))
        {
            return UpdateStaffMemberAvailabilityResult.Failure(UpdateStaffMemberAvailabilityError.InvalidDayOfWeek);
        }

        if (command.StartTime >= command.EndTime)
        {
            return UpdateStaffMemberAvailabilityResult.Failure(UpdateStaffMemberAvailabilityError.InvalidTimeRange);
        }

        if (await availabilityRepository.OverlapsAsync(
                command.StaffMemberId,
                dayOfWeek,
                command.StartTime,
                command.EndTime,
                command.AvailabilityId,
                cancellationToken))
        {
            return UpdateStaffMemberAvailabilityResult.Failure(UpdateStaffMemberAvailabilityError.AvailabilityOverlaps);
        }

        availability.DayOfWeek = dayOfWeek;
        availability.StartTime = command.StartTime;
        availability.EndTime = command.EndTime;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UpdateStaffMemberAvailabilityResult.Success(MapAvailability(availability));
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
