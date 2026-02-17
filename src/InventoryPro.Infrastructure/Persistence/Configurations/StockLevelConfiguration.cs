using InventoryPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("StockLevels");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Quantity)
            .HasDefaultValue(0);

        builder.Property(s => s.ReservedQuantity)
            .HasDefaultValue(0);

        builder.Property(s => s.LastUpdated)
            .HasDefaultValueSql("NOW()");

        // Unique index for product-variant-warehouse combination
        builder.HasIndex(s => new { s.ProductId, s.ProductVariantId, s.WarehouseId })
            .IsUnique();

        builder.HasOne(s => s.Product)
            .WithMany(p => p.StockLevels)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ProductVariant)
            .WithMany(v => v.StockLevels)
            .HasForeignKey(s => s.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Warehouse)
            .WithMany(w => w.StockLevels)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.WarehouseZone)
            .WithMany(z => z.StockLevels)
            .HasForeignKey(s => s.WarehouseZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ignore computed property
        builder.Ignore(s => s.AvailableQuantity);
    }
}
