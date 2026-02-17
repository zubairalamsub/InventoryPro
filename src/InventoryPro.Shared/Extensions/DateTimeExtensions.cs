namespace InventoryPro.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        var diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
        return dateTime.AddDays(-1 * diff).Date;
    }

    public static DateTime EndOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        return dateTime.StartOfWeek(startOfWeek).AddDays(7).AddTicks(-1);
    }

    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return dateTime.StartOfYear().AddYears(1).AddTicks(-1);
    }

    public static string ToRelativeTime(this DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;

        if (span.TotalDays > 365)
            return $"{(int)(span.TotalDays / 365)} year(s) ago";

        if (span.TotalDays > 30)
            return $"{(int)(span.TotalDays / 30)} month(s) ago";

        if (span.TotalDays > 1)
            return $"{(int)span.TotalDays} day(s) ago";

        if (span.TotalHours > 1)
            return $"{(int)span.TotalHours} hour(s) ago";

        if (span.TotalMinutes > 1)
            return $"{(int)span.TotalMinutes} minute(s) ago";

        return "Just now";
    }
}
