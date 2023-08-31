namespace TestDbContainer;

internal static class Helpers
{
    public static bool NotHasValue(this DateTime? dt) => !dt.HasValue;
}
