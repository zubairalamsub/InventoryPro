using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class ExchangeRate : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid BaseCurrencyId { get; set; }
    public Guid TargetCurrencyId { get; set; }
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Currency BaseCurrency { get; set; } = null!;
    public virtual Currency TargetCurrency { get; set; } = null!;
}
