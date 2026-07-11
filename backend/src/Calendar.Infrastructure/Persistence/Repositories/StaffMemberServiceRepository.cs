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

    public void Add(StaffMemberService staffMemberService) => dbContext.StaffMemberServices.Add(staffMemberService);
}
