#region

using System.Text.RegularExpressions;

#endregion

namespace LavaSharp.Helpers;

public class RegexTemplates
{
    public static readonly Regex SoundcloudUrl = new(@"^https?:\/\/(soundcloud\.com|snd\.sc)\/(.*)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex YouTubeUrl =
        new(
            @"^((?:https?:)?\/\/)?((?:www|m|music)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex SpotifyUrl =
        new(@"https?://(open\.spotify\.com|spotify\.link)(/track)?/([^ ?&])+(\?si\=[^ ?&]*)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex Url =
        new(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
}