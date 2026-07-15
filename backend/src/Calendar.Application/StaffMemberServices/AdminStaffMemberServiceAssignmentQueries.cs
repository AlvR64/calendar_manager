using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMemberServices;

public sealed record ListStaffMemberServiceAssignmentsByStaffMemberQuery(
    Guid BusinessId,
    Guid StaffMemberId) : IQuery<ListStaffMemberServiceAssignmentsResult>;

public sealed record ListStaffMemberServiceAssignmentsByServiceQuery(
    Guid BusinessId,
    Guid ServiceId) : IQuery<ListStaffMemberServiceAssignmentsResult>;

public sealed class AdminStaffMemberServiceAssignmentQueryHandler(
    IBusinessRepository businessRepository,
    IStaffMemberRepository staffMemberRepository,
    IServiceRepository serviceRepository,
    IStaffMemberServiceRepository staffMemberServiceRepository)
    : IQueryHandler<ListStaffMemberServiceAssignmentsByStaffMemberQuery, ListStaffMemberServiceAssignmentsResult>,
        IQueryHandler<ListStaffMemberServiceAssignmentsByServiceQuery, ListStaffMemberServiceAssignmentsResult>
{
    public async Task<ListStaffMemberServiceAssignmentsResult> HandleAsync(
        ListStaffMemberServiceAssignmentsByStaffMemberQuery query,
        CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsByIdAsync(query.BusinessId, cancellationToken))
        {
            return ListStaffMemberServiceAssignmentsResult.Failure(ListStaffMemberServiceAssignmentsError.BusinessNotFound);
        }

        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(query.StaffMemberId, query.BusinessId, cancellationToken))
        {
            return ListStaffMemberServiceAssignmentsResult.Failure(ListStaffMemberServiceAssignmentsError.StaffMemberNotFound);
        }

        var assignments = await staffMemberServiceRepository.ListByBusinessIdAndStaffMemberIdAsync(
            query.BusinessId,
            query.StaffMemberId,
            cancellationToken);

        return ListStaffMemberServiceAssignmentsResult.Success(assignments.Select(MapAssignment).ToList());
    }

    public async Task<ListStaffMemberServiceAssignmentsResult> HandleAsync(
        ListStaffMemberServiceAssignmentsByServiceQuery query,
        CancellationToken cancellationToken)
    {
        if (!await businessRepository.ExistsByIdAsync(query.BusinessId, cancellationToken))
        {
            return ListStaffMemberServiceAssignmentsResult.Failure(ListStaffMemberServiceAssignmentsError.BusinessNotFound);
        }

        if (!await serviceRepository.ExistsByIdAndBusinessIdAsync(query.ServiceId, query.BusinessId, cancellationToken))
        {
            return ListStaffMemberServiceAssignmentsResult.Failure(ListStaffMemberServiceAssignmentsError.ServiceNotFound);
        }

        var assignments = await staffMemberServiceRepository.ListByBusinessIdAndServiceIdAsync(
            query.BusinessId,
            query.ServiceId,
            cancellationToken);

        return ListStaffMemberServiceAssignmentsResult.Success(assignments.Select(MapAssignment).ToList());
    }

    private static StaffMemberServiceAssignmentDetails MapAssignment(StaffMemberService assignment) => new(
        assignment.StaffMemberId,
        assignment.ServiceId,
        assignment.IsActive,
        assignment.CreatedAtUtc);
}

public sealed record StaffMemberServiceAssignmentDetails(
    Guid StaffMemberId,
    Guid ServiceId,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);

public enum ListStaffMemberServiceAssignmentsError
{
    None = 0,
    BusinessNotFound = 1,
    StaffMemberNotFound = 2,
    ServiceNotFound = 3
}

public sealed record ListStaffMemberServiceAssignmentsResult(
    bool Succeeded,
    ListStaffMemberServiceAssignmentsError Error,
    IReadOnlyList<StaffMemberServiceAssignmentDetails> Assignments)
{
    public static ListStaffMemberServiceAssignmentsResult Success(IReadOnlyList<StaffMemberServiceAssignmentDetails> assignments) => new(
        true,
        ListStaffMemberServiceAssignmentsError.None,
        assignments);

    public static ListStaffMemberServiceAssignmentsResult Failure(ListStaffMemberServiceAssignmentsError error) => new(
        false,
        error,
        []);
}
