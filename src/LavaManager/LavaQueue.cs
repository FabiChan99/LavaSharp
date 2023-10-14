#region

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using DisCatSharp.Lavalink.EventArgs;
using LavaSharp.Config;
using LavaSharp.Helpers;

#endregion

namespace LavaSharp.LavaManager
{
    public static class LavaQueue
    {
        public static Queue<(LavalinkTrack, DiscordUser)> queue = new();
        public static bool isLooping = false;

        public static async Task DisconnectAndReset(LavalinkGuildPlayer connection)
        {
            queue.Clear();
            isLooping = false;
            CurrentPlayData.track = null;
            CurrentPlayData.player = null;
            CurrentPlayData.user = null;
            await connection.DisconnectAsync();
        }

        public static async Task PlaybackFinished(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs e,
            InteractionContext ctx)
        {
            if (e.Reason == LavalinkTrackEndReason.Replaced)
            {
                return;
            }

            if (isLooping)
            {
                await sender.PlayAsync(e.Track);
                CurrentPlayData.track = e.Track;
                CurrentPlayData.player = sender;
                await ctx.Channel.SendMessageAsync("🔂 | Looping is active. Looping current track!");
                await ctx.Channel.SendMessageAsync(EmbedGenerator.GetCurrentTrackEmbed(e.Track, sender));
                return;
            }

            if (queue.Count > 0)
            {
                var nextTrack = queue.Dequeue();
                CurrentPlayData.track = nextTrack.Item1;
                CurrentPlayData.user = nextTrack.Item2;
                CurrentPlayData.player = sender;
                await sender.PlayAsync(nextTrack.Item1);
                await ctx.Channel.SendMessageAsync(EmbedGenerator.GetCurrentTrackEmbed(nextTrack.Item1, sender));
            }

            if (sender.CurrentTrack == null && queue.Count == 0 && e.Reason != LavalinkTrackEndReason.Stopped)
            {
                var finishedemb = new DiscordEmbedBuilder
                {
                    Title = "🎵 | Queue finished!",
                    Description = "The queue has finished playing. Disconnecting...",
                    Color = BotConfig.GetEmbedColor()
                };
                await ctx.Channel.SendMessageAsync(finishedemb);
                await DisconnectAndReset(sender);
            }
        }
    }
}