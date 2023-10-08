namespace LavaSharp.Helpers;

public class TimeParser
{
    public static bool TryParseTime(string input, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        string[] parts = input.Split(':');

        if (parts.Length != 3)
        {
            return false;
        }

        if (int.TryParse(parts[0], out int hours) &&
            int.TryParse(parts[1], out int minutes) &&
            int.TryParse(parts[2], out int seconds))
        {
            time = new TimeSpan(hours, minutes, seconds);
            return true;
        }

        return false;
    }
}