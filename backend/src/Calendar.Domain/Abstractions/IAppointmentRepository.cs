using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IAppointmentRepository
{
    Task<IReadOnlyList<Appointment>> ListBlockingAppointmentsAsync(
        Guid businessId,
        IReadOnlyCollection<Guid> staffMemberIds,
        DateTimeOffset rangeStartUtc,
        DateTimeOffset rangeEndUtc,
        CancellationToken cancellationToken);

    Task<bool> HasBlockingOverlapAsync(
        Guid businessId,
        Guid staffMemberId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid? excludedAppointmentId,
        CancellationToken cancellationToken);
}
