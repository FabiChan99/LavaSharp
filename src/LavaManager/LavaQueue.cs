#region

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using DisCatSharp.Lavalink.EventArgs;
using LavaSharp.Helpers;

#endregion

namespace LavaSharp.LavaManager;

public static class LavaQueue
{
    public static Queue<(LavalinkTrack, DiscordUser)> queue = new();
    public static bool isLooping;
    public static bool isPaused;

    public static async Task DisconnectAndReset(LavalinkGuildPlayer connection)
    {
        queue.Clear();
        isLooping = false;
        isPaused = false;
        CurrentPlayData.track = null;
        CurrentPlayData.player = null;
        CurrentPlayData.user = null;
        CurrentPlayData.CurrentVolume = 100;
        await NowPlaying.TryRemoveButtonsFromMessage(CurrentPlayData.CurrentNowPlayingMessageId);
        CurrentPlayData.CurrentExecutionChannel = null!;
        CurrentPlayData.CurrentNowPlayingMessageId = 0;
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
            await NowPlaying.sendNowPlayingTrack(ctx, e.Track);
            return;
        }

        if (queue.Count > 0)
        {
            var nextTrack = queue.Dequeue();
            CurrentPlayData.track = nextTrack.Item1;
            CurrentPlayData.user = nextTrack.Item2;
            CurrentPlayData.player = sender;
            await sender.PlayAsync(nextTrack.Item1);
            await NowPlaying.sendNowPlayingTrack(ctx, nextTrack.Item1);
        }

        if (sender.CurrentTrack == null && queue.Count == 0 && e.Reason != LavalinkTrackEndReason.Stopped)
        {
            await ctx.Channel.SendMessageAsync("⏹️ | Queue is empty. Stopping player and leaving voice channel.");
            await DisconnectAndReset(sender);
        }
    }
}