using Calendar.Domain.Entities;

namespace Calendar.Domain.Abstractions;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);

    Task<Appointment?> GetByIdWithDetailsForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Appointment>> ListByCustomerIdWithDetailsAsync(
        Guid customerId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        AppointmentStatus? status,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Appointment>> ListByBusinessIdWithDetailsAsync(
        Guid businessId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        Guid? staffMemberId,
        Guid? serviceId,
        AppointmentStatus? status,
        CancellationToken cancellationToken);

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
