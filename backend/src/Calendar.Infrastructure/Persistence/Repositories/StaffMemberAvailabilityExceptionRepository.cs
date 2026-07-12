using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class StaffMemberAvailabilityExceptionRepository(CalendarDbContext dbContext) : IStaffMemberAvailabilityExceptionRepository
{
    public async Task<IReadOnlyList<StaffMemberAvailabilityException>> ListByStaffMemberIdAsync(
        Guid staffMemberId,
        CancellationToken cancellationToken) =>
        await dbContext.StaffMemberAvailabilityExceptions
            .AsNoTracking()
            .Where(exception => exception.StaffMemberId == staffMemberId)
            .OrderBy(exception => exception.LocalDate)
            .ThenBy(exception => exception.StartTime)
            .ToListAsync(cancellationToken);

    public Task<StaffMemberAvailabilityException?> GetByIdAndStaffMemberIdForUpdateAsync(
        Guid id,
        Guid staffMemberId,
        CancellationToken cancellationToken) =>
        dbContext.StaffMemberAvailabilityExceptions.FirstOrDefaultAsync(
            exception => exception.Id == id && exception.StaffMemberId == staffMemberId,
            cancellationToken);

    public Task<bool> HasAnyExceptionForDateAsync(
        Guid staffMemberId,
        DateOnly localDate,
        Guid? excludedExceptionId,
        CancellationToken cancellationToken) =>
        dbContext.StaffMemberAvailabilityExceptions.AnyAsync(
            exception => exception.StaffMemberId == staffMemberId
                && exception.LocalDate == localDate
                && (!excludedExceptionId.HasValue || exception.Id != excludedExceptionId.Value),
            cancellationToken);

    public Task<bool> HasClosedExceptionAsync(
        Guid staffMemberId,
        DateOnly localDate,
        Guid? excludedExceptionId,
        CancellationToken cancellationToken) =>
        dbContext.StaffMemberAvailabilityExceptions.AnyAsync(
            exception => exception.StaffMemberId == staffMemberId
                && exception.LocalDate == localDate
                && exception.IsClosed
                && (!excludedExceptionId.HasValue || exception.Id != excludedExceptionId.Value),
            cancellationToken);

    public Task<bool> OverlapsAsync(
        Guid staffMemberId,
        DateOnly localDate,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludedExceptionId,
        CancellationToken cancellationToken) =>
        dbContext.StaffMemberAvailabilityExceptions.AnyAsync(
            exception => exception.StaffMemberId == staffMemberId
                && exception.LocalDate == localDate
                && (!excludedExceptionId.HasValue || exception.Id != excludedExceptionId.Value)
                && !exception.IsClosed
                && exception.StartTime.HasValue
                && exception.EndTime.HasValue
                && exception.StartTime.Value < endTime
                && startTime < exception.EndTime.Value,
            cancellationToken);

    public void Add(StaffMemberAvailabilityException exception) => dbContext.StaffMemberAvailabilityExceptions.Add(exception);

    public void Remove(StaffMemberAvailabilityException exception) => dbContext.StaffMemberAvailabilityExceptions.Remove(exception);
}
