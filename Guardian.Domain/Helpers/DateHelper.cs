namespace Guardian.Domain.Helpers
{
    internal class DateHelper
    {
        public static DateTimeOffset GetTimeZoneDate(string? tz = null)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(tz ?? "E. Australia Standard Time");
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        }
    }
}