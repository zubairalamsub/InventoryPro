using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ExchangeRate> BaseCurrencyRates { get; set; } = new List<ExchangeRate>();
    public virtual ICollection<ExchangeRate> TargetCurrencyRates { get; set; } = new List<ExchangeRate>();
}
