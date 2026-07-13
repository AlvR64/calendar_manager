using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(CalendarDbContext dbContext) : IAppointmentRepository
{
    public async Task<IReadOnlyList<Appointment>> ListBlockingAppointmentsAsync(
        Guid businessId,
        IReadOnlyCollection<Guid> staffMemberIds,
        DateTimeOffset rangeStartUtc,
        DateTimeOffset rangeEndUtc,
        CancellationToken cancellationToken) =>
        await dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.BusinessId == businessId
                && staffMemberIds.Contains(appointment.StaffMemberId)
                && appointment.Status != AppointmentStatus.CancelledByCustomer
                && appointment.Status != AppointmentStatus.CancelledByAdmin
                && appointment.StartAtUtc < rangeEndUtc
                && rangeStartUtc < appointment.EndAtUtc)
            .OrderBy(appointment => appointment.StartAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasBlockingOverlapAsync(
        Guid businessId,
        Guid staffMemberId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        Guid? excludedAppointmentId,
        CancellationToken cancellationToken) =>
        await dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(appointment => appointment.BusinessId == businessId
                && appointment.StaffMemberId == staffMemberId
                && appointment.Status != AppointmentStatus.CancelledByCustomer
                && appointment.Status != AppointmentStatus.CancelledByAdmin
                && (!excludedAppointmentId.HasValue || appointment.Id != excludedAppointmentId.Value)
                && appointment.StartAtUtc < endAtUtc
                && startAtUtc < appointment.EndAtUtc,
                cancellationToken);
}
