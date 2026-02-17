using System.Text;

namespace InventoryPro.Shared.Helpers;

public static class SkuGenerator
{
    private static readonly Random Random = new();

    public static string Generate(string? prefix = null, int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            sb.Append(prefix.ToUpperInvariant().Replace(" ", ""));
            sb.Append('-');
        }

        for (var i = 0; i < length; i++)
        {
            sb.Append(chars[Random.Next(chars.Length)]);
        }

        return sb.ToString();
    }

    public static string GenerateFromName(string name, int maxLength = 10)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Generate();

        var words = name.ToUpperInvariant()
            .Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

        var sb = new StringBuilder();

        foreach (var word in words)
        {
            if (sb.Length >= maxLength - 4)
                break;

            sb.Append(word.Length >= 3 ? word[..3] : word);
        }

        sb.Append('-');
        sb.Append(Random.Next(1000, 9999));

        return sb.ToString();
    }
}
