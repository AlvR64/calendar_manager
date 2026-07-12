using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class StaffMemberServiceRepository(CalendarDbContext dbContext) : IStaffMemberServiceRepository
{
    public Task<bool> ExistsAsync(Guid staffMemberId, Guid serviceId, CancellationToken cancellationToken) =>
        dbContext.StaffMemberServices.AnyAsync(
            staffMemberService => staffMemberService.StaffMemberId == staffMemberId
                && staffMemberService.ServiceId == serviceId,
            cancellationToken);

    public async Task<IReadOnlyList<StaffMemberService>> ListActiveByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken) =>
        await dbContext.StaffMemberServices
            .AsNoTracking()
            .Where(staffMemberService => staffMemberService.IsActive
                && staffMemberService.StaffMember.BusinessId == businessId
                && staffMemberService.StaffMember.IsActive
                && staffMemberService.Service.BusinessId == businessId
                && staffMemberService.Service.IsActive)
            .OrderBy(staffMemberService => staffMemberService.StaffMember.SortOrder)
            .ThenBy(staffMemberService => staffMemberService.Service.SortOrder)
            .ToListAsync(cancellationToken);

    public void Add(StaffMemberService staffMemberService) => dbContext.StaffMemberServices.Add(staffMemberService);
}
