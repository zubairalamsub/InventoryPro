using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class GoodsReceivedNote : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string GrnNumber { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public Guid ReceivedBy { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual ICollection<GoodsReceivedNoteItem> Items { get; set; } = new List<GoodsReceivedNoteItem>();
}
