using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class UnitOfMeasure : BaseEntity
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsSystemDefault { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
