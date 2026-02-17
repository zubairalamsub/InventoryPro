using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class SupplierProduct : BaseEntity
{
    public Guid SupplierId { get; set; }
    public Guid ProductId { get; set; }
    public string? SupplierSKU { get; set; }
    public decimal UnitCost { get; set; }
    public int MinOrderQuantity { get; set; } = 1;
    public int? LeadTimeDays { get; set; }
    public bool IsPreferred { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
