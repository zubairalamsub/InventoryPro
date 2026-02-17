using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class TaxConfiguration : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsCompound { get; set; }
    public bool IsInclusive { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
