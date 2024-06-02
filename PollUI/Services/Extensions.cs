namespace Poll.Services;

public static class Extensions
{
    public static string ToSqlFormat(this DateTimeOffset date)
    {
        // 2024-02-16 21:55:35.850722 +00:00
        return $"{date:yyyy-MM-dd hh:mm:ss.fff K}";
    }

    public static int? FindIndex<T>(this IEnumerable<T> enumerable, Func<T, bool> func)
    {
        var match = enumerable.Select((element, index) => new { element, index }).FirstOrDefault(i=>func(i.element));
        return match?.index;
    }
}