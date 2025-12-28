namespace OrderManagement.Blazor.Helpers;

/// <summary>
/// Helper for formatting DateTimeOffset to local time
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Formats DateTimeOffset to local date and time string
    /// </summary>
    public static string ToLocalDateTime(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
    }
}
