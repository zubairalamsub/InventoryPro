using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class Payment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? SaleId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
    public Guid ProcessedBy { get; set; }
    public DateTime ProcessedAt { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Sale? Sale { get; set; }
    public virtual PurchaseOrder? PurchaseOrder { get; set; }
}
