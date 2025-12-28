namespace OrderManagement.Application.Validators;

public static class ValidationHelpers
{
    private static readonly char[] DangerousCharacters = { '<', '>', '&', '"', '\'' };

    public static bool IsSanitized(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return !value.Any(c => DangerousCharacters.Contains(c));
    }

    public static string GetDangerousCharactersMessage()
    {
        return "Input contains dangerous characters (< > & \" ')";
    }
}
