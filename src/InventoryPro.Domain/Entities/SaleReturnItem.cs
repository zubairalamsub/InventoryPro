using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class SaleReturnItem : BaseEntity
{
    public Guid SaleReturnId { get; set; }
    public Guid SaleItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal RefundAmount { get; set; }
    public bool ReturnToStock { get; set; } = true;
    public Guid? WarehouseId { get; set; }
    public string? Reason { get; set; }

    // Navigation properties
    public virtual SaleReturn SaleReturn { get; set; } = null!;
    public virtual SaleItem SaleItem { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual Warehouse? Warehouse { get; set; }
}
