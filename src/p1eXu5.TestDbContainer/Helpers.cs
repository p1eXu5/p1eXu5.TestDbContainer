namespace TestDbContainer;

internal static class Helpers
{
    public static bool HasNoValue(this DateTime? dt) => !dt.HasValue;
}
