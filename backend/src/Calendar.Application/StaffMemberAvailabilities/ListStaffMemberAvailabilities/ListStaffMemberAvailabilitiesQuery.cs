using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilities;

namespace Calendar.Application.StaffMemberAvailabilities.ListStaffMemberAvailabilities;

public sealed record ListStaffMemberAvailabilitiesQuery(Guid BusinessId, Guid StaffMemberId) : IQuery<ListStaffMemberAvailabilitiesResult>;
