using System.Reflection;
using InventoryPro.Domain.Common;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly ITenantProvider? _tenantProvider;

    // This property is evaluated at query time, not at model build time
    public Guid? CurrentTenantId => _tenantProvider?.GetTenantId();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // Core entities
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Products
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    // Inventory
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseZone> WarehouseZones => Set<WarehouseZone>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<BatchLot> BatchLots => Set<BatchLot>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentItem> StockAdjustmentItems => Set<StockAdjustmentItem>();

    // Suppliers & Purchasing
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierProduct> SupplierProducts => Set<SupplierProduct>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceivedNote> GoodsReceivedNotes => Set<GoodsReceivedNote>();
    public DbSet<GoodsReceivedNoteItem> GoodsReceivedNoteItems => Set<GoodsReceivedNoteItem>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();

    // Customers & Sales
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
    public DbSet<SaleReturnItem> SaleReturnItems => Set<SaleReturnItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<HeldTransaction> HeldTransactions => Set<HeldTransaction>();

    // Discounts & Pricing
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();
    public DbSet<TaxGroup> TaxGroups => Set<TaxGroup>();

    // Configuration
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<InvoiceSetting> InvoiceSettings => Set<InvoiceSetting>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // Notifications & Logging
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    // Subscriptions
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<BillingHistory> BillingHistories => Set<BillingHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from the assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply global query filter for multi-tenancy
        ApplyTenantQueryFilters(builder);

        // Apply soft delete filter
        ApplySoftDeleteFilters(builder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder builder)
    {
        // Apply tenant filter to all entities implementing ITenantEntity
        // The filter uses CurrentTenantId property which is evaluated at query time
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                    .MakeGenericMethod(entityType.ClrType);

                method?.Invoke(null, new object[] { builder, this });
            }
        }
    }

    private static void ApplyTenantFilter<T>(ModelBuilder builder, ApplicationDbContext context) where T : class, ITenantEntity
    {
        // This filter is evaluated at query time using the CurrentTenantId property
        builder.Entity<T>().HasQueryFilter(e =>
            context.CurrentTenantId == null || e.TenantId == context.CurrentTenantId);
    }

    private static void ApplySoftDeleteFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseAuditableEntity.IsDeleted));
                var isNotDeleted = System.Linq.Expressions.Expression.Equal(isDeletedProperty, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(isNotDeleted, parameter);

                // Check if there's already a query filter and combine them
                var existingFilter = entityType.GetQueryFilter();
                if (existingFilter != null)
                {
                    var body = System.Linq.Expressions.Expression.AndAlso(
                        System.Linq.Expressions.Expression.Invoke(existingFilter, parameter),
                        isNotDeleted);
                    lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                }

                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
