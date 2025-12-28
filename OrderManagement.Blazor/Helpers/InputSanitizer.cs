using System.Text;

namespace OrderManagement.Blazor.Helpers;

public static class InputSanitizer
{
    private static readonly char[] DangerousCharacters = { '<', '>', '&', '"', '\'' };

    public static string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        if (!ContainsDangerousCharacters(input))
            return input;

        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (!DangerousCharacters.Contains(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public static bool ContainsDangerousCharacters(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return input.Any(c => DangerousCharacters.Contains(c));
    }

    public static string HtmlEncode(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    public static string GetValidationMessage()
    {
        return "Input contains dangerous characters (< > & \" ')";
    }
}
