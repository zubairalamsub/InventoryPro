using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class StockAdjustment : BaseEntity, ITenantEntity, IAggregateRoot
{
    public Guid TenantId { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public AdjustmentReason Reason { get; set; }
    public string? Notes { get; set; }
    public Guid AdjustedBy { get; set; }
    public DateTime AdjustmentDate { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
