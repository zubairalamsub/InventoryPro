using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class TaxGroup : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid[] TaxConfigurationIds { get; set; } = Array.Empty<Guid>();
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
}
