using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class StaffMemberAvailabilityRepository(CalendarDbContext dbContext) : IStaffMemberAvailabilityRepository
{
    public async Task<IReadOnlyList<StaffMemberAvailability>> ListByStaffMemberIdAsync(
        Guid staffMemberId,
        CancellationToken cancellationToken) =>
        await dbContext.StaffMemberAvailabilities
            .AsNoTracking()
            .Where(availability => availability.StaffMemberId == staffMemberId)
            .OrderBy(availability => availability.DayOfWeek)
            .ThenBy(availability => availability.StartTime)
            .ToListAsync(cancellationToken);

    public Task<StaffMemberAvailability?> GetByIdAndStaffMemberIdForUpdateAsync(
        Guid id,
        Guid staffMemberId,
        CancellationToken cancellationToken) =>
        dbContext.StaffMemberAvailabilities.FirstOrDefaultAsync(
            availability => availability.Id == id && availability.StaffMemberId == staffMemberId,
            cancellationToken);

    public Task<bool> OverlapsAsync(
        Guid staffMemberId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludedAvailabilityId,
        CancellationToken cancellationToken) =>
        dbContext.StaffMemberAvailabilities.AnyAsync(
            availability => availability.StaffMemberId == staffMemberId
                && availability.DayOfWeek == dayOfWeek
                && availability.IsActive
                && (!excludedAvailabilityId.HasValue || availability.Id != excludedAvailabilityId.Value)
                && availability.StartTime < endTime
                && startTime < availability.EndTime,
            cancellationToken);

    public void Add(StaffMemberAvailability availability) => dbContext.StaffMemberAvailabilities.Add(availability);

    public void Remove(StaffMemberAvailability availability) => dbContext.StaffMemberAvailabilities.Remove(availability);
}
