using Calendar.Application.Abstractions.Messaging;
using Calendar.Domain.Abstractions;

namespace Calendar.Application.Appointments.GetAppointmentDetails;

public sealed class GetAppointmentDetailsQueryHandler(
    IAppointmentRepository appointmentRepository,
    ITimeZoneProvider timeZoneProvider) : IQueryHandler<GetAppointmentDetailsQuery, AppointmentDetails?>
{
    public async Task<AppointmentDetails?> HandleAsync(GetAppointmentDetailsQuery query, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdWithDetailsAsync(query.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return null;
        }

        return AppointmentDetailsMapper.Map(appointment, timeZoneProvider);
    }
}
