using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class Quotation : BaseAuditableEntity, ITenantEntity, IAggregateRoot
{
    public Guid TenantId { get; set; }
    public Guid? CustomerId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public Guid? ConvertedToSaleId { get; set; }

    public bool IsExpired => ValidUntil.HasValue && ValidUntil.Value < DateTime.UtcNow;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Customer? Customer { get; set; }
    public virtual Sale? ConvertedToSale { get; set; }
    public virtual ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
}
