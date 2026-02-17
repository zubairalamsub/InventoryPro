using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace InventoryPro.Shared.Helpers;

public static partial class SlugHelper
{
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase
        var slug = text.ToLowerInvariant();

        // Remove diacritics (accents)
        slug = RemoveDiacritics(slug);

        // Replace spaces with hyphens
        slug = slug.Replace(' ', '-');

        // Remove invalid characters
        slug = InvalidCharsRegex().Replace(slug, string.Empty);

        // Replace multiple hyphens with single hyphen
        slug = MultipleHyphensRegex().Replace(slug, "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        return slug;
    }

    public static string GenerateUniqueSlug(string text, Func<string, bool> slugExists)
    {
        var baseSlug = GenerateSlug(text);

        if (!slugExists(baseSlug))
            return baseSlug;

        var counter = 1;
        string newSlug;

        do
        {
            newSlug = $"{baseSlug}-{counter}";
            counter++;
        } while (slugExists(newSlug));

        return newSlug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}
