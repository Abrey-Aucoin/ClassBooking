namespace ClassBooking.Web.Helpers;

public static class StudioTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

    public static DateTime ToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, Zone);

    public static DateTime ToUtc(DateTime local) => TimeZoneInfo.ConvertTimeToUtc(local, Zone);
}