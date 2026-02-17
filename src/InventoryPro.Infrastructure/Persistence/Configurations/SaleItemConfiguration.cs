using InventoryPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.ProductName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(si => si.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(si => si.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(si => si.CostPrice)
            .HasPrecision(18, 4);

        builder.Property(si => si.DiscountPercent)
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(si => si.DiscountAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(si => si.TaxRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(si => si.TaxAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(si => si.LineTotal)
            .HasPrecision(18, 4);

        builder.HasOne(si => si.Sale)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.Product)
            .WithMany()
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.ProductVariant)
            .WithMany()
            .HasForeignKey(si => si.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(si => si.BatchLot)
            .WithMany()
            .HasForeignKey(si => si.BatchLotId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ignore computed property
        builder.Ignore(si => si.Profit);
    }
}
