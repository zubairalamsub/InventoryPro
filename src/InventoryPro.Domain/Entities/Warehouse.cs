using InventoryPro.Domain.Common;
using InventoryPro.Domain.Common.ValueObjects;

namespace InventoryPro.Domain.Entities;

public class Warehouse : BaseEntity, ITenantEntity, IAggregateRoot
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Address? Address { get; set; }
    public Guid? ManagerId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ApplicationUser? Manager { get; set; }
    public virtual ICollection<WarehouseZone> Zones { get; set; } = new List<WarehouseZone>();
    public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
