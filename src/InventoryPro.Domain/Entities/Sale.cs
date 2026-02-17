using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class Sale : BaseAuditableEntity, ITenantEntity, IAggregateRoot
{
    public Guid TenantId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? QuotationId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public Guid? DiscountId { get; set; }
    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public int LoyaltyPointsEarned { get; set; }
    public int LoyaltyPointsRedeemed { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public Guid CashierId { get; set; }
    public DateTime SaleDate { get; set; }

    public decimal OutstandingAmount => TotalAmount - PaidAmount;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Customer? Customer { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual Quotation? Quotation { get; set; }
    public virtual Discount? Discount { get; set; }
    public virtual Coupon? Coupon { get; set; }
    public virtual ApplicationUser Cashier { get; set; } = null!;
    public virtual ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<SaleReturn> Returns { get; set; } = new List<SaleReturn>();
}
