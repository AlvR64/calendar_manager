using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);

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

    void Add(Appointment appointment);
}
