using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilities;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMemberAvailabilities.ListStaffMemberAvailabilities;

public sealed class ListStaffMemberAvailabilitiesQueryHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberAvailabilityRepository availabilityRepository)
    : IQueryHandler<ListStaffMemberAvailabilitiesQuery, ListStaffMemberAvailabilitiesResult>
{
    public async Task<ListStaffMemberAvailabilitiesResult> HandleAsync(
        ListStaffMemberAvailabilitiesQuery query,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(query.StaffMemberId, query.BusinessId, cancellationToken))
        {
            return ListStaffMemberAvailabilitiesResult.Failure(ListStaffMemberAvailabilitiesError.StaffMemberNotFound);
        }

        var availabilities = await availabilityRepository.ListByStaffMemberIdAsync(query.StaffMemberId, cancellationToken);
        return ListStaffMemberAvailabilitiesResult.Success(availabilities.Select(MapAvailability).ToList());
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
