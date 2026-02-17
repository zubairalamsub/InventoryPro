using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class WarehouseZone : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}
