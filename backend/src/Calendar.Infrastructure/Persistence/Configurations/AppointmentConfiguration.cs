using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments", table =>
        {
            table.HasCheckConstraint("CK_Appointments_EndAtUtc_After_StartAtUtc", "[EndAtUtc] > [StartAtUtc]");
            table.HasCheckConstraint("CK_Appointments_ServiceDurationSnapshot_Positive", "[ServiceDurationMinutesSnapshot] > 0");
            table.HasCheckConstraint("CK_Appointments_PriceAmountSnapshot_NonNegative", "[PriceAmountSnapshot] >= 0");
        });

        builder.HasKey(appointment => appointment.Id);
        builder.Property(appointment => appointment.Status).HasConversion<int>();
        builder.Property(appointment => appointment.CustomerNotes).HasMaxLength(1000);
        builder.Property(appointment => appointment.InternalNotes).HasMaxLength(1000);
        builder.Property(appointment => appointment.ServiceNameSnapshot).HasMaxLength(150).IsRequired();
        builder.Property(appointment => appointment.PriceAmountSnapshot).HasPrecision(18, 2);
        builder.Property(appointment => appointment.CurrencyCodeSnapshot).HasMaxLength(3).IsRequired();
        builder.Property(appointment => appointment.CancellationReason).HasMaxLength(500);

        builder.HasIndex(appointment => new { appointment.BusinessId, appointment.StartAtUtc });
        builder.HasIndex(appointment => new { appointment.BusinessId, appointment.Status, appointment.StartAtUtc });
        builder.HasIndex(appointment => new { appointment.StaffMemberId, appointment.StartAtUtc });
        builder.HasIndex(appointment => new { appointment.CustomerId, appointment.StartAtUtc });
        builder.HasIndex(appointment => new { appointment.ServiceId, appointment.StartAtUtc });

        builder.HasOne(appointment => appointment.Business)
            .WithMany(business => business.Appointments)
            .HasForeignKey(appointment => appointment.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(appointment => appointment.StaffMember)
            .WithMany(staffMember => staffMember.Appointments)
            .HasForeignKey(appointment => appointment.StaffMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(appointment => appointment.Service)
            .WithMany(service => service.Appointments)
            .HasForeignKey(appointment => appointment.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(appointment => appointment.Customer)
            .WithMany(customer => customer.Appointments)
            .HasForeignKey(appointment => appointment.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
