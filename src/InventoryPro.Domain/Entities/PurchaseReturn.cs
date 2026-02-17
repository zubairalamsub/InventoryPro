using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class PurchaseReturn : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid SupplierId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}
