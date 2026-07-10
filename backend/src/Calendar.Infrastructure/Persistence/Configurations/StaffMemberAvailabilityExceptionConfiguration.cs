using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class StaffMemberAvailabilityExceptionConfiguration : IEntityTypeConfiguration<StaffMemberAvailabilityException>
{
    public void Configure(EntityTypeBuilder<StaffMemberAvailabilityException> builder)
    {
        builder.ToTable("StaffMemberAvailabilityExceptions", table =>
        {
            table.HasCheckConstraint(
                "CK_StaffMemberAvailabilityExceptions_TimeRange_When_Open",
                "([IsClosed] = 1 AND [StartTime] IS NULL AND [EndTime] IS NULL) OR ([IsClosed] = 0 AND [StartTime] IS NOT NULL AND [EndTime] IS NOT NULL AND [EndTime] > [StartTime])");
        });

        builder.HasKey(exception => exception.Id);
        builder.Property(exception => exception.Reason).HasMaxLength(250);

        builder.HasIndex(exception => new { exception.StaffMemberId, exception.LocalDate });

        builder.HasOne(exception => exception.StaffMember)
            .WithMany(staffMember => staffMember.AvailabilityExceptions)
            .HasForeignKey(exception => exception.StaffMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
