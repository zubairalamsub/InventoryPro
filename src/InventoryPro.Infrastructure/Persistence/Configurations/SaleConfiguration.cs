using InventoryPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.SubTotal)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(s => s.TaxAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(s => s.DiscountAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(s => s.ShippingAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(s => s.TotalAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(s => s.PaidAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(s => s.ChangeAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(s => s.CouponCode)
            .HasMaxLength(50);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(s => s.SaleDate)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(s => new { s.TenantId, s.InvoiceNumber })
            .IsUnique();

        builder.HasIndex(s => new { s.TenantId, s.SaleDate });
        builder.HasIndex(s => new { s.TenantId, s.CashierId });
        builder.HasIndex(s => new { s.TenantId, s.CustomerId });

        // Relationships
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Warehouse)
            .WithMany(w => w.Sales)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Cashier)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Discount)
            .WithMany()
            .HasForeignKey(s => s.DiscountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Coupon)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Quotation)
            .WithOne(q => q.ConvertedToSale)
            .HasForeignKey<Sale>(s => s.QuotationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Sale)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Returns)
            .WithOne(r => r.Sale)
            .HasForeignKey(r => r.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed property
        builder.Ignore(s => s.OutstandingAmount);
    }
}
