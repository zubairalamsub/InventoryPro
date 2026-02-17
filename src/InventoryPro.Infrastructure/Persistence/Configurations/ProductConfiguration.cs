using InventoryPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Slug)
            .HasMaxLength(300);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(500);

        builder.Property(p => p.CostPrice)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(p => p.SellingPrice)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(p => p.WholesalePrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.MinimumPrice)
            .HasPrecision(18, 4);

        builder.Property(p => p.Weight)
            .HasPrecision(10, 3);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.TrackInventory)
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Unique index for SKU within tenant (excluding deleted)
        builder.HasIndex(p => new { p.TenantId, p.SKU })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(p => new { p.TenantId, p.Barcode });
        builder.HasIndex(p => new { p.TenantId, p.CategoryId });

        // Relationships
        builder.HasOne(p => p.Tenant)
            .WithMany(t => t.Products)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.UnitOfMeasure)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.TaxConfiguration)
            .WithMany(t => t.Products)
            .HasForeignKey(p => p.TaxConfigurationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.StockLevels)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
