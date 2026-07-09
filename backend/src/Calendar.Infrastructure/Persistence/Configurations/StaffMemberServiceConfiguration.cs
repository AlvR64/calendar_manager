using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class StaffMemberServiceConfiguration : IEntityTypeConfiguration<StaffMemberService>
{
    public void Configure(EntityTypeBuilder<StaffMemberService> builder)
    {
        builder.ToTable("StaffMemberServices");

        builder.HasKey(staffMemberService => new
        {
            staffMemberService.StaffMemberId,
            staffMemberService.ServiceId
        });

        builder.Property(staffMemberService => staffMemberService.IsActive).HasDefaultValue(true);

        builder.HasIndex(staffMemberService => staffMemberService.ServiceId);

        builder.HasOne(staffMemberService => staffMemberService.StaffMember)
            .WithMany(staffMember => staffMember.StaffMemberServices)
            .HasForeignKey(staffMemberService => staffMemberService.StaffMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(staffMemberService => staffMemberService.Service)
            .WithMany(service => service.StaffMemberServices)
            .HasForeignKey(staffMemberService => staffMemberService.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
