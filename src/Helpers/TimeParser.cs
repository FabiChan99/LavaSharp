namespace LavaSharp.Helpers
{
    public class TimeParser
    {
        public static bool TryParseTime(string input, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            string[] parts = input.Split(':');

            if (parts.Length != 2)
            {
                return false;
            }

            if (int.TryParse(parts[0], out int minutes) &&
                int.TryParse(parts[1], out int seconds))
            {
                time = new TimeSpan(0, minutes, seconds);
                return true;
            }

            return false;
        }
    }
}