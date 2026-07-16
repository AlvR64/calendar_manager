using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Appointments.GetAppointmentDetails;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Appointments.ListCustomerAppointments;

public sealed class ListCustomerAppointmentsQueryHandler(
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider) : IQueryHandler<ListCustomerAppointmentsQuery, IReadOnlyList<AppointmentDetails>>
{
    public async Task<IReadOnlyList<AppointmentDetails>> HandleAsync(ListCustomerAppointmentsQuery query, CancellationToken cancellationToken)
    {
        var appointments = await appointmentRepository.ListByCustomerIdWithDetailsAsync(
            query.CustomerId,
            query.FromUtc?.ToUniversalTime(),
            query.ToUtc?.ToUniversalTime(),
            query.Status,
            cancellationToken);

        return appointments
            .Select(appointment => AppointmentDetailsMapper.Map(appointment, timeZoneProvider))
            .ToList();
    }
}
