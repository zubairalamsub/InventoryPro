using System.Text.RegularExpressions;

namespace InventoryPro.Domain.Common.ValueObjects;

public partial record PhoneNumber
{
    public string? CountryCode { get; init; }
    public string Number { get; init; } = string.Empty;

    private PhoneNumber() { }

    public PhoneNumber(string number, string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Phone number is required", nameof(number));

        var cleanNumber = CleanPhoneNumber(number);

        if (cleanNumber.Length < 7 || cleanNumber.Length > 15)
            throw new ArgumentException("Invalid phone number length", nameof(number));

        Number = cleanNumber;
        CountryCode = countryCode?.TrimStart('+');
    }

    private static string CleanPhoneNumber(string phoneNumber)
    {
        return PhoneNumberCleanupRegex().Replace(phoneNumber, string.Empty);
    }

    public string GetFormattedNumber()
    {
        if (!string.IsNullOrWhiteSpace(CountryCode))
            return $"+{CountryCode} {Number}";

        return Number;
    }

    public override string ToString() => GetFormattedNumber();

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex PhoneNumberCleanupRegex();
}
