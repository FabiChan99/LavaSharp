namespace LavaSharp.Helpers;

public class TimeParser
{
    public static bool TryParseTime(string input, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        string[] parts = input.Split(':');

        if (parts.Length != 2 && parts.Length != 3)
        {
            return false;
        }

        if (int.TryParse(parts[0], out int hours) &&
            int.TryParse(parts[1], out int minutes))
        {
            int seconds = parts.Length == 3 ? (int.TryParse(parts[2], out seconds) ? seconds : 0) : 0;
            time = new TimeSpan(hours, minutes, seconds);
            return true;
        }

        return false;
    }
}
