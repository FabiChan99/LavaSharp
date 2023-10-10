using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;

namespace LavaSharp.Helpers;

public class SongResolver
{
    public static string GetTrackInfo(LavalinkTrack track)
    {
        return $"{track.Info.Title} ({track.Info.Length:mm\\:ss})";
    }

}

public static class CurrentPlayData
{
    public static LavalinkTrack? track { get; set; }
    public static LavalinkGuildPlayer? player { get; set; }
    public static DiscordUser? user { get; set; }
    public static int CurrentVolume { get; set; } = 100;
}
