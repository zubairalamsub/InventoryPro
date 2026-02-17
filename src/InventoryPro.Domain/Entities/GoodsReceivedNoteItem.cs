using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class GoodsReceivedNoteItem : BaseEntity
{
    public Guid GoodsReceivedNoteId { get; set; }
    public Guid PurchaseOrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityAccepted { get; set; }
    public int QuantityRejected { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? BatchLotId { get; set; }

    // Navigation properties
    public virtual GoodsReceivedNote GoodsReceivedNote { get; set; } = null!;
    public virtual PurchaseOrderItem PurchaseOrderItem { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual BatchLot? BatchLot { get; set; }
}
