namespace InventoryPro.Shared.Constants;

public static class CacheKeys
{
    public const string TenantPrefix = "tenant";
    public const string ProductPrefix = "product";
    public const string CategoryPrefix = "category";
    public const string CustomerPrefix = "customer";
    public const string SettingsPrefix = "settings";
    public const string SubscriptionPrefix = "subscription";

    public static string Tenant(Guid tenantId) => $"{TenantPrefix}:{tenantId}";
    public static string TenantBySubdomain(string subdomain) => $"{TenantPrefix}:subdomain:{subdomain}";
    public static string TenantByDomain(string domain) => $"{TenantPrefix}:domain:{domain}";

    public static string Product(Guid tenantId, Guid productId) => $"{ProductPrefix}:{tenantId}:{productId}";
    public static string ProductBySku(Guid tenantId, string sku) => $"{ProductPrefix}:{tenantId}:sku:{sku}";
    public static string ProductByBarcode(Guid tenantId, string barcode) => $"{ProductPrefix}:{tenantId}:barcode:{barcode}";
    public static string ProductList(Guid tenantId) => $"{ProductPrefix}:{tenantId}:list";

    public static string Categories(Guid tenantId) => $"{CategoryPrefix}:{tenantId}:all";
    public static string CategoryTree(Guid tenantId) => $"{CategoryPrefix}:{tenantId}:tree";

    public static string Customer(Guid tenantId, Guid customerId) => $"{CustomerPrefix}:{tenantId}:{customerId}";

    public static string TenantSettings(Guid tenantId) => $"{SettingsPrefix}:{tenantId}";
    public static string InvoiceSettings(Guid tenantId) => $"{SettingsPrefix}:{tenantId}:invoice";
    public static string TaxConfigurations(Guid tenantId) => $"{SettingsPrefix}:{tenantId}:tax";

    public static string TenantSubscription(Guid tenantId) => $"{SubscriptionPrefix}:{tenantId}";
    public static string SubscriptionPlans() => $"{SubscriptionPrefix}:plans";
}
