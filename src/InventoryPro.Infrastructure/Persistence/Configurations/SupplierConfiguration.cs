using InventoryPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(s => s.Code)
            .HasMaxLength(50);

        builder.Property(s => s.ContactPerson)
            .HasMaxLength(200);

        builder.Property(s => s.Email)
            .HasMaxLength(255);

        builder.Property(s => s.Phone)
            .HasMaxLength(20);

        builder.Property(s => s.AlternatePhone)
            .HasMaxLength(20);

        builder.Property(s => s.TaxIdentificationNo)
            .HasMaxLength(100);

        builder.Property(s => s.Website)
            .HasMaxLength(255);

        builder.Property(s => s.PaymentTerms)
            .HasMaxLength(200);

        builder.Property(s => s.CreditLimit)
            .HasPrecision(18, 4);

        builder.Property(s => s.CurrentBalance)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(s => s.Rating)
            .HasPrecision(3, 2);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.OwnsOne(s => s.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(500);
            address.Property(a => a.Street2).HasColumnName("Address_Street2").HasMaxLength(500);
            address.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("Address_State").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100);
        });

        builder.HasIndex(s => s.TenantId);

        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Suppliers)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.SupplierProducts)
            .WithOne(sp => sp.Supplier)
            .HasForeignKey(sp => sp.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.PurchaseOrders)
            .WithOne(po => po.Supplier)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
