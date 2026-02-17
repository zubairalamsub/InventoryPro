using InventoryPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryPro.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.BillingCycle)
            .HasMaxLength(20)
            .HasDefaultValue("monthly");

        builder.Property(s => s.StripeCustomerId)
            .HasMaxLength(255);

        builder.Property(s => s.StripeSubscriptionId)
            .HasMaxLength(255);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(s => s.TenantId);

        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Subscriptions)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.BillingHistories)
            .WithOne(b => b.Subscription)
            .HasForeignKey(b => b.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed property
        builder.Ignore(s => s.IsTrialActive);
    }
}
