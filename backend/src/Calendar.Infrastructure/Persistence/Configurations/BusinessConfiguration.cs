using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses");

        builder.HasKey(business => business.Id);

        builder.Property(business => business.Name).HasMaxLength(150).IsRequired();
        builder.Property(business => business.Slug).HasMaxLength(120).IsRequired();
        builder.Property(business => business.Description).HasMaxLength(1000);
        builder.Property(business => business.ContactEmail).HasMaxLength(255);
        builder.Property(business => business.ContactPhoneNumber).HasMaxLength(30);
        builder.Property(business => business.WebsiteUrl).HasMaxLength(500);
        builder.Property(business => business.AddressLine1).HasMaxLength(200);
        builder.Property(business => business.AddressLine2).HasMaxLength(200);
        builder.Property(business => business.City).HasMaxLength(100);
        builder.Property(business => business.PostalCode).HasMaxLength(20);
        builder.Property(business => business.CountryCode).HasMaxLength(2);
        builder.Property(business => business.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(business => business.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(business => business.IsActive).HasDefaultValue(true);

        builder.HasIndex(business => business.Slug).IsUnique();
    }
}
