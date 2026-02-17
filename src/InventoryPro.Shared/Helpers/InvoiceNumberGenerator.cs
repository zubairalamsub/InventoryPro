namespace InventoryPro.Shared.Helpers;

public static class InvoiceNumberGenerator
{
    public static string Generate(string prefix, int sequenceNumber, int padLength = 6)
    {
        return $"{prefix}{sequenceNumber.ToString().PadLeft(padLength, '0')}";
    }

    public static string GenerateWithDate(string prefix, int sequenceNumber, DateTime date)
    {
        return $"{prefix}{date:yyyyMMdd}-{sequenceNumber:D4}";
    }

    public static string GenerateWithYearMonth(string prefix, int sequenceNumber, DateTime date)
    {
        return $"{prefix}{date:yyyyMM}-{sequenceNumber:D5}";
    }
}
