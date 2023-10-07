namespace LavaSharp.Helpers;

public static class HexCheck
{
    public static bool IsHexColor(string color)
    {
        if (color.Length != 6) return false;

        foreach (char c in color)
            if (!IsHexDigit(c))
                return false;

        return true;
    }

    private static bool IsHexDigit(char c)
    {
        return (c >= '0' && c <= '9') ||
               (c >= 'A' && c <= 'F') ||
               (c >= 'a' && c <= 'f');
    }
}