using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Watchbook.Utils;

public static class SlugHelper
{
    public static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // 1. Normalize Unicode characters (remove accents)
        text = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in text)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        text = sb.ToString().Normalize(NormalizationForm.FormC);

        // 2. Convert to lowercase
        text = text.ToLowerInvariant();

        // 3. Remove emojis and other non-alphanumeric symbols
        text = Regex.Replace(text, @"[^\w\s-]", "");

        // 4. Replace spaces and underscores with hyphens
        text = Regex.Replace(text, @"[\s_]+", "-");

        // 5. Collapse multiple hyphens
        text = Regex.Replace(text, @"-+", "-");

        // 6. Trim leading/trailing hyphens
        text = text.Trim('-');

        return text;
    }
}

// Example usage
class Program
{
    static void Main()
    {
        string title1 = "My Movie: The Beginning!";
        string title2 = "Épisode 12 – L'été ☀️";
        string title3 = "  Hello__World  ";

        Console.WriteLine(SlugHelper.Slugify(title1)); // my-movie-the-beginning
        Console.WriteLine(SlugHelper.Slugify(title2)); // episode-12-lete
        Console.WriteLine(SlugHelper.Slugify(title3)); // hello-world
    }
}
