using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services", table =>
        {
            table.HasCheckConstraint("CK_Services_DurationMinutes_Positive", "[DurationMinutes] > 0");
            table.HasCheckConstraint("CK_Services_PriceAmount_NonNegative", "[PriceAmount] >= 0");
        });

        builder.HasKey(service => service.Id);
        builder.Property(service => service.Name).HasMaxLength(150).IsRequired();
        builder.Property(service => service.Description).HasMaxLength(1000);
        builder.Property(service => service.PriceAmount).HasPrecision(18, 2);
        builder.Property(service => service.IsActive).HasDefaultValue(true);

        builder.HasIndex(service => new { service.BusinessId, service.IsActive });
        builder.HasIndex(service => new { service.BusinessId, service.SortOrder });

        builder.HasOne(service => service.Business)
            .WithMany(business => business.Services)
            .HasForeignKey(service => service.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
