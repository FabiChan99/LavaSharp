using DisCatSharp.Lavalink.Entities;
using LavaSharp.Config;

namespace LavaSharp.Helpers;

public class SongResolver
{
    public static string GetTrackInfo(LavalinkTrack track)
    {
        return $"{track.Info.Title} ({track.Info.Length:mm\\:ss})";
    }
}