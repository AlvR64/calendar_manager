using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class StaffMemberAvailabilityConfiguration : IEntityTypeConfiguration<StaffMemberAvailability>
{
    public void Configure(EntityTypeBuilder<StaffMemberAvailability> builder)
    {
        builder.ToTable("StaffMemberAvailabilities", table =>
        {
            table.HasCheckConstraint("CK_StaffMemberAvailabilities_EndTime_After_StartTime", "[EndTime] > [StartTime]");
        });

        builder.HasKey(availability => availability.Id);
        builder.Property(availability => availability.DayOfWeek).HasConversion<int>();
        builder.Property(availability => availability.IsActive).HasDefaultValue(true);

        builder.HasIndex(availability => new { availability.StaffMemberId, availability.DayOfWeek });

        builder.HasOne(availability => availability.StaffMember)
            .WithMany(staffMember => staffMember.Availabilities)
            .HasForeignKey(availability => availability.StaffMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
