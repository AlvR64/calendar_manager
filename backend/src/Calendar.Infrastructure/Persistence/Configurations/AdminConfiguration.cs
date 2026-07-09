using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("Admins");

        builder.HasKey(admin => admin.Id);

        builder.Property(admin => admin.Email).HasMaxLength(255).IsRequired();
        builder.Property(admin => admin.NormalizedEmail).HasMaxLength(255).IsRequired();
        builder.Property(admin => admin.PasswordHash).IsRequired();
        builder.Property(admin => admin.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(admin => admin.IsActive).HasDefaultValue(true);

        builder.HasIndex(admin => admin.BusinessId).IsUnique();
        builder.HasIndex(admin => admin.NormalizedEmail).IsUnique();

        builder.HasOne(admin => admin.Business)
            .WithOne(business => business.Admin)
            .HasForeignKey<Admin>(admin => admin.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
