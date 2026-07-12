using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.StaffMemberAvailabilityExceptions;
using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.StaffMemberAvailabilityExceptions.ListStaffMemberAvailabilityExceptions;

public sealed class ListStaffMemberAvailabilityExceptionsQueryHandler(
    IStaffMemberRepository staffMemberRepository,
    IStaffMemberAvailabilityExceptionRepository availabilityExceptionRepository)
    : IQueryHandler<ListStaffMemberAvailabilityExceptionsQuery, ListStaffMemberAvailabilityExceptionsResult>
{
    public async Task<ListStaffMemberAvailabilityExceptionsResult> HandleAsync(
        ListStaffMemberAvailabilityExceptionsQuery query,
        CancellationToken cancellationToken)
    {
        if (!await staffMemberRepository.ExistsByIdAndBusinessIdAsync(query.StaffMemberId, query.BusinessId, cancellationToken))
        {
            return ListStaffMemberAvailabilityExceptionsResult.Failure(ListStaffMemberAvailabilityExceptionsError.StaffMemberNotFound);
        }

        var exceptions = await availabilityExceptionRepository.ListByStaffMemberIdAsync(query.StaffMemberId, cancellationToken);
        return ListStaffMemberAvailabilityExceptionsResult.Success(exceptions.Select(MapException).ToList());
    }

    private static StaffMemberAvailabilityExceptionDetails MapException(StaffMemberAvailabilityException exception) => new(
        exception.Id,
        exception.StaffMemberId,
        exception.LocalDate,
        exception.IsClosed,
        exception.StartTime,
        exception.EndTime,
        exception.Reason,
        exception.CreatedAtUtc);
}
