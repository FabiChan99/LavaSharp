using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink;

internal class QueueEntry : IQueueEntry
{
    public LavalinkTrack Track { get; set; }

    public Task AfterPlayingAsync(LavalinkGuildPlayer player)
    {
        player.RemoveFromQueue(this);
        return Task.CompletedTask;
    }

    public async Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player)
    {
        await player.Channel.SendMessageAsync($"Playing {this.Track.Info.Title}");
        return true;
    }

}