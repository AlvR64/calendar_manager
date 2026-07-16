using Calendar.Domain.Abstractions;
using Calendar.Domain.Entities;

namespace Calendar.Application.Appointments.GetAppointmentDetails;

public static class AppointmentDetailsMapper
{
    public static AppointmentDetails Map(Appointment appointment, ITimeZoneProvider timeZoneProvider)
    {
        if (!timeZoneProvider.TryGetIanaTimeZoneInfo(appointment.Business.TimeZoneId, out var businessTimeZone))
        {
            businessTimeZone = TimeZoneInfo.Utc;
        }

        var localStart = TimeZoneInfo.ConvertTime(appointment.StartAtUtc, businessTimeZone);
        var localEnd = TimeZoneInfo.ConvertTime(appointment.EndAtUtc, businessTimeZone);

        return new AppointmentDetails(
            appointment.Id,
            new AppointmentBusinessDetails(
                appointment.BusinessId,
                appointment.Business.Name,
                appointment.Business.Slug,
                appointment.Business.TimeZoneId),
            new AppointmentServiceDetails(
                appointment.ServiceId,
                appointment.ServiceNameSnapshot,
                appointment.ServiceDurationMinutesSnapshot,
                appointment.PriceAmountSnapshot,
                appointment.CurrencyCodeSnapshot),
            new AppointmentStaffMemberDetails(
                appointment.StaffMemberId,
                appointment.StaffMember.DisplayName),
            new AppointmentCustomerDetails(
                appointment.CustomerId,
                appointment.Customer.FirstName,
                appointment.Customer.LastName,
                appointment.Customer.Email),
            appointment.StartAtUtc,
            appointment.EndAtUtc,
            DateOnly.FromDateTime(localStart.DateTime),
            TimeOnly.FromDateTime(localStart.DateTime),
            TimeOnly.FromDateTime(localEnd.DateTime),
            appointment.Status.ToString(),
            appointment.CustomerNotes,
            appointment.InternalNotes,
            appointment.CancelledAtUtc,
            appointment.CancellationReason,
            appointment.CreatedAtUtc);
    }
}
