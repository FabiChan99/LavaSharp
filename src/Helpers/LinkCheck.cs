using System.Text.RegularExpressions;
using DisCatSharp.Lavalink.Entities;
using LavaSharp.Enums;

namespace LavaSharp.Helpers;
public class LinkCheck
{
    public static bool isLink(string query)
    {
        // check if link is a youtube, spotify, or soundcloud link
        string youtubeRegex = @"^(https?\:\/\/)?(www\.youtube\.com|youtu\.?be)\/.+$";
        string spotifyRegex = @"^(https?\:\/\/)?(open\.spotify\.com)\/.+$";
        string soundcloudRegex = @"^(https?\:\/\/)?(soundcloud\.com)\/.+$";

        if (Regex.IsMatch(query, youtubeRegex) || Regex.IsMatch(query, spotifyRegex) ||
            Regex.IsMatch(query, soundcloudRegex))
        {
            return true;
        }
        return false;
    }

    public static SourceType GetSourceType(string query)
    {
        string youtubeRegex = @"^(https?\:\/\/)?(www\.youtube\.com|youtu\.?be)\/.+$";
        string spotifyRegex = @"^(https?\:\/\/)?(open\.spotify\.com)\/.+$";
        string soundcloudRegex = @"^(https?\:\/\/)?(soundcloud\.com)\/.+$";

        if (Regex.IsMatch(query, youtubeRegex))
        {
            return SourceType.YouTube;
        }
        else if (Regex.IsMatch(query, spotifyRegex))
        {
            return SourceType.Spotify;
        }
        else if (Regex.IsMatch(query, soundcloudRegex))
        {
            return SourceType.SoundCloud;
        }
        else
        {
            return SourceType.YouTube;
        }
    }

}