using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(CalendarDbContext dbContext) : IAppointmentRepository
{
    public async Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Business)
            .Include(appointment => appointment.Service)
            .Include(appointment => appointment.StaffMember)
            .Include(appointment => appointment.Customer)
            .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);

    public async Task<Appointment?> GetByIdWithDetailsForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Appointments
            .Include(appointment => appointment.Business)
            .Include(appointment => appointment.Service)
            .Include(appointment => appointment.StaffMember)
            .Include(appointment => appointment.Customer)
            .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> ListByCustomerIdWithDetailsAsync(
        Guid customerId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Business)
            .Include(appointment => appointment.Service)
            .Include(appointment => appointment.StaffMember)
            .Include(appointment => appointment.Customer)
            .Where(appointment => appointment.CustomerId == customerId);

        if (fromUtc.HasValue)
        {
            query = query.Where(appointment => appointment.EndAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(appointment => appointment.StartAtUtc <= toUtc.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(appointment => appointment.Status == status.Value);
        }

        return await query
            .OrderBy(appointment => appointment.StartAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> ListByBusinessIdWithDetailsAsync(
        Guid businessId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        Guid? staffMemberId,
        Guid? serviceId,
        AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Business)
            .Include(appointment => appointment.Service)
            .Include(appointment => appointment.StaffMember)
            .Include(appointment => appointment.Customer)
            .Where(appointment => appointment.BusinessId == businessId)
            .Where(appointment => appointment.StartAtUtc < toUtc && fromUtc < appointment.EndAtUtc);

        if (staffMemberId.HasValue)
        {
            query = query.Where(appointment => appointment.StaffMemberId == staffMemberId.Value);
        }

        if (serviceId.HasValue)
        {
            query = query.Where(appointment => appointment.ServiceId == serviceId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(appointment => appointment.Status == status.Value);
        }

        return await query
            .OrderBy(appointment => appointment.StartAtUtc)
            .ToListAsync(cancellationToken);
    }

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

    public void Add(Appointment appointment) => dbContext.Appointments.Add(appointment);
}
