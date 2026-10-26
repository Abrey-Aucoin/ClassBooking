using System.Text.RegularExpressions;

namespace ClassBooking.Web.Helpers;

public static class DisplayText
{
    public static string Spaced(this Enum value) =>
        Regex.Replace(value.ToString(), "(?<=[a-z])(?=[A-Z])", " ");

    public static string ToClassTime(this DateTime value) =>
        value.ToString("dddd, MMMM d 'at' h:mm tt");

    public static string ToShortClassTime(this DateTime value) =>
        value.ToString("ddd, MMM d, h:mm tt");
}
