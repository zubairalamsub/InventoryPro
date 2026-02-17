using System.Globalization;

namespace InventoryPro.Shared.Extensions;

public static class DecimalExtensions
{
    public static string ToCurrency(this decimal value, string currencySymbol = "$", int decimalPlaces = 2)
    {
        return $"{currencySymbol}{value.ToString($"N{decimalPlaces}")}";
    }

    public static string ToCurrency(this decimal value, CultureInfo culture)
    {
        return value.ToString("C", culture);
    }

    public static decimal RoundTo(this decimal value, int decimalPlaces)
    {
        return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
    }

    public static decimal RoundUp(this decimal value, int decimalPlaces = 2)
    {
        var multiplier = (decimal)Math.Pow(10, decimalPlaces);
        return Math.Ceiling(value * multiplier) / multiplier;
    }

    public static decimal RoundDown(this decimal value, int decimalPlaces = 2)
    {
        var multiplier = (decimal)Math.Pow(10, decimalPlaces);
        return Math.Floor(value * multiplier) / multiplier;
    }

    public static decimal CalculatePercentage(this decimal value, decimal percentage)
    {
        return value * percentage / 100;
    }

    public static decimal CalculatePercentageOf(this decimal part, decimal whole)
    {
        return whole == 0 ? 0 : part / whole * 100;
    }

    public static decimal AddPercentage(this decimal value, decimal percentage)
    {
        return value + value.CalculatePercentage(percentage);
    }

    public static decimal SubtractPercentage(this decimal value, decimal percentage)
    {
        return value - value.CalculatePercentage(percentage);
    }

    public static decimal CalculateTax(this decimal amount, decimal taxRate, bool isInclusive = false)
    {
        if (isInclusive)
        {
            return amount - (amount / (1 + taxRate / 100));
        }

        return amount * taxRate / 100;
    }
}
