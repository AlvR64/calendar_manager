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

    public void Add(StaffMember staffMember) => dbContext.StaffMembers.Add(staffMember);
}
