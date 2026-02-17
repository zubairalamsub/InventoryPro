namespace InventoryPro.Shared.Constants;

public static class AppConstants
{
    public const string ApplicationName = "InventoryPro";
    public const string ApiVersion = "v1";

    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        public const int MinPageSize = 1;
    }

    public static class Cache
    {
        public const int ShortDurationMinutes = 5;
        public const int MediumDurationMinutes = 30;
        public const int LongDurationMinutes = 60;
        public const int VeryLongDurationMinutes = 1440; // 24 hours
    }

    public static class File
    {
        public const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
        public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public static readonly string[] AllowedImportExtensions = { ".xlsx", ".xls", ".csv" };
    }

    public static class Loyalty
    {
        public const decimal PointsPerCurrencyUnit = 1; // 1 point per $1 spent
        public const decimal CurrencyValuePerPoint = 0.01m; // Each point worth $0.01
    }

    public static class Invoice
    {
        public const string DefaultPrefix = "INV-";
        public const string QuotationPrefix = "QUO-";
        public const string PurchaseOrderPrefix = "PO-";
        public const string GrnPrefix = "GRN-";
        public const string ReturnPrefix = "RET-";
        public const string TransferPrefix = "TRF-";
        public const string AdjustmentPrefix = "ADJ-";
    }
}
