using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IStaffMemberRepository
{
    void Add(StaffMember staffMember);
}
