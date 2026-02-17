using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class BillingHistory : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public string? InvoiceUrl { get; set; }
    public string? StripeInvoiceId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Subscription Subscription { get; set; } = null!;
}
