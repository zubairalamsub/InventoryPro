using InventoryPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(po => po.SubTotal)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(po => po.TaxAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(po => po.ShippingCost)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(po => po.DiscountAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(po => po.TotalAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(po => po.PaidAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(po => po.OrderDate)
            .HasDefaultValueSql("NOW()");

        builder.Property(po => po.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(po => new { po.TenantId, po.Status });
        builder.HasIndex(po => new { po.TenantId, po.OrderNumber })
            .IsUnique();

        builder.HasOne(po => po.Tenant)
            .WithMany()
            .HasForeignKey(po => po.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Warehouse)
            .WithMany()
            .HasForeignKey(po => po.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(po => po.GoodsReceivedNotes)
            .WithOne(grn => grn.PurchaseOrder)
            .HasForeignKey(grn => grn.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(po => po.Payments)
            .WithOne(p => p.PurchaseOrder)
            .HasForeignKey(p => p.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed property
        builder.Ignore(po => po.OutstandingAmount);
    }
}
