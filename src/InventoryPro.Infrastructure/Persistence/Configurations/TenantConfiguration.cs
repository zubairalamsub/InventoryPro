using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Subdomain)
            .IsUnique();

        builder.Property(t => t.CustomDomain)
            .HasMaxLength(255);

        builder.HasIndex(t => t.CustomDomain)
            .IsUnique()
            .HasFilter("\"CustomDomain\" IS NOT NULL");

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.FaviconUrl)
            .HasMaxLength(500);

        builder.Property(t => t.PrimaryColor)
            .HasMaxLength(7)
            .HasDefaultValue("#1976D2");

        builder.Property(t => t.SecondaryColor)
            .HasMaxLength(7)
            .HasDefaultValue("#424242");

        builder.Property(t => t.BusinessType)
            .HasMaxLength(100);

        builder.Property(t => t.BusinessRegistrationNo)
            .HasMaxLength(100);

        builder.Property(t => t.TaxIdentificationNo)
            .HasMaxLength(100);

        builder.Property(t => t.Phone)
            .HasMaxLength(20);

        builder.Property(t => t.Email)
            .HasMaxLength(255);

        builder.Property(t => t.Website)
            .HasMaxLength(255);

        builder.OwnsOne(t => t.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(500);
            address.Property(a => a.Street2).HasColumnName("Address_Street2").HasMaxLength(500);
            address.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("Address_State").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100);
        });

        builder.Property(t => t.TimeZone)
            .HasMaxLength(100)
            .HasDefaultValue("UTC");

        builder.Property(t => t.FinancialYearStart)
            .HasDefaultValue(1);

        builder.Property(t => t.ValuationMethod)
            .HasDefaultValue(ValuationMethod.WeightedAverage);

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(t => t.DefaultCurrency)
            .WithMany()
            .HasForeignKey(t => t.DefaultCurrencyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
