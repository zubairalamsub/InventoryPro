using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class QuotationItem : BaseEntity
{
    public Guid QuotationId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation properties
    public virtual Quotation Quotation { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ProductVariant? ProductVariant { get; set; }
}
