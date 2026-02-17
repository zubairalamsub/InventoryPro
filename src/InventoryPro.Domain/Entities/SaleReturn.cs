using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class SaleReturn : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid SaleId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalRefundAmount { get; set; }
    public PaymentMethod RefundMethod { get; set; }
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
    public Guid ProcessedBy { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Sale Sale { get; set; } = null!;
    public virtual ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();
}
