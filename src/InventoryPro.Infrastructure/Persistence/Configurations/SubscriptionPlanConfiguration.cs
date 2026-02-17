using InventoryPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sp => sp.MonthlyPrice)
            .HasPrecision(18, 4);

        builder.Property(sp => sp.AnnualPrice)
            .HasPrecision(18, 4);

        builder.Property(sp => sp.Features)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(sp => sp.IsActive)
            .HasDefaultValue(true);

        builder.Property(sp => sp.SortOrder)
            .HasDefaultValue(0);
    }
}
