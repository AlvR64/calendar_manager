using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class StaffMemberRepository(CalendarDbContext dbContext) : IStaffMemberRepository
{
    public Task<bool> ExistsByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) =>
        dbContext.StaffMembers.AnyAsync(
            staffMember => staffMember.Id == id && staffMember.BusinessId == businessId,
            cancellationToken);

    public async Task<IReadOnlyList<StaffMember>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) =>
        await dbContext.StaffMembers
            .AsNoTracking()
            .Where(staffMember => staffMember.BusinessId == businessId && staffMember.IsActive)
            .OrderBy(staffMember => staffMember.SortOrder)
            .ThenBy(staffMember => staffMember.DisplayName)
            .ToListAsync(cancellationToken);

    public Task<StaffMember?> GetActiveByIdAndBusinessIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken) =>
        dbContext.StaffMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                staffMember => staffMember.Id == id
                    && staffMember.BusinessId == businessId
                    && staffMember.IsActive
                    && staffMember.Business.IsActive,
                cancellationToken);

    public void Add(StaffMember staffMember) => dbContext.StaffMembers.Add(staffMember);
}
