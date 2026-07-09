using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.ToTable("StaffMembers");

        builder.HasKey(staffMember => staffMember.Id);
        builder.Property(staffMember => staffMember.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(staffMember => staffMember.Email).HasMaxLength(255);
        builder.Property(staffMember => staffMember.PhoneNumber).HasMaxLength(30);
        builder.Property(staffMember => staffMember.Bio).HasMaxLength(1000);
        builder.Property(staffMember => staffMember.IsActive).HasDefaultValue(true);

        builder.HasIndex(staffMember => new { staffMember.BusinessId, staffMember.IsActive });
        builder.HasIndex(staffMember => new { staffMember.BusinessId, staffMember.SortOrder });

        builder.HasOne(staffMember => staffMember.Business)
            .WithMany(business => business.StaffMembers)
            .HasForeignKey(staffMember => staffMember.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
