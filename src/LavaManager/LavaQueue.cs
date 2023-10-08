using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using DisCatSharp.Lavalink.EventArgs;
using LavaSharp.Config;
using LavaSharp.Helpers;

namespace LavaSharp.LavaManager
{
    public static class LavaQueue
    {
        public static Queue<LavalinkTrack> queue = new();
        public static bool isLooping = false;

        public static async Task DisconnectAndReset(LavalinkGuildPlayer connection)
        {
            await connection.DisconnectAsync();
            queue.Clear();
            isLooping = false;
        }

        public static async Task PlaybackFinished(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs e, InteractionContext ctx)
        {
            if (e.Reason == LavalinkTrackEndReason.Replaced)
            {
                return;
            }

            if (isLooping)
            {
                await sender.PlayAsync(e.Track);

                var eb = EmbedGenerator.GetPlayEmbed(e.Track);
                eb.WithDescription($"Looping: {SongResolver.GetTrackInfo(e.Track)}");
                eb.WithTitle("Looping Track");

                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(eb));
            }
            else if (queue.Count > 0)
            {
                var nextTrack = queue.Dequeue();
                await sender.PlayAsync(nextTrack);

                int remainingTracks = queue.Count;
                string message = $"Playing next track: {SongResolver.GetTrackInfo(nextTrack)}\n";

                var eb = EmbedGenerator.GetPlayEmbed(nextTrack);
                eb.WithDescription(message);
                eb.WithTitle("Now Playing");

                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(eb));
            }
            else if (sender.CurrentTrack == null)
            {
                await DisconnectAndReset(sender);
            }
        }
    }
}
