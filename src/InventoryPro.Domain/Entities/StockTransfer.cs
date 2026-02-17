using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class StockTransfer : BaseEntity, ITenantEntity, IAggregateRoot
{
    public Guid TenantId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Pending;
    public string? Notes { get; set; }
    public Guid TransferredBy { get; set; }
    public Guid? ReceivedBy { get; set; }
    public DateTime TransferDate { get; set; }
    public DateTime? ReceivedDate { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Warehouse FromWarehouse { get; set; } = null!;
    public virtual Warehouse ToWarehouse { get; set; } = null!;
    public virtual ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
