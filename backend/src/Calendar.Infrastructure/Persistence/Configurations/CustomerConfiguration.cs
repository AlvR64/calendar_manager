using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Email).HasMaxLength(255).IsRequired();
        builder.Property(customer => customer.NormalizedEmail).HasMaxLength(255).IsRequired();
        builder.Property(customer => customer.PasswordHash).IsRequired();
        builder.Property(customer => customer.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.LastName).HasMaxLength(100);
        builder.Property(customer => customer.PhoneNumber).HasMaxLength(30);
        builder.Property(customer => customer.IsActive).HasDefaultValue(true);

        builder.HasIndex(customer => customer.NormalizedEmail).IsUnique();
    }
}
