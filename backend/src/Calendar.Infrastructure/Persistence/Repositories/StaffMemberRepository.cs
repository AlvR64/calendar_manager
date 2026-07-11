using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class StaffMemberRepository(CalendarDbContext dbContext) : IStaffMemberRepository
{
    public void Add(StaffMember staffMember) => dbContext.StaffMembers.Add(staffMember);
}
