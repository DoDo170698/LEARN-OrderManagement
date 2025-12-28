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

    /// <summary>
    /// Formats DateTimeOffset to local date string
    /// </summary>
    public static string ToLocalDate(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToLocalTime().ToString("dd/MM/yyyy");
    }

    /// <summary>
    /// Formats DateTimeOffset to local time string
    /// </summary>
    public static string ToLocalTime(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToLocalTime().ToString("HH:mm:ss");
    }

    /// <summary>
    /// Formats DateTimeOffset to relative time (e.g., "2 hours ago")
    /// </summary>
    public static string ToRelativeTime(DateTimeOffset dateTimeOffset)
    {
        var timeSpan = DateTimeOffset.UtcNow - dateTimeOffset;

        if (timeSpan.TotalSeconds < 60)
            return $"{(int)timeSpan.TotalSeconds} giây trước";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} phút trước";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} giờ trước";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} ngày trước";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";

        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";

        return $"{(int)(timeSpan.TotalDays / 365)} năm trước";
    }
}
