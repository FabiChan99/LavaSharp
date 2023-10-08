using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.EventArgs;
using LavaSharp.Config;
using LavaSharp.Helpers;

namespace LavaSharp.LavaManager
{
    public static class LavaQueue
    {
        public static Queue<LavalinkTrack> queue = new Queue<LavalinkTrack>();
        public static bool isLooping = false;




        public static async Task DisconnectAndReset(LavalinkGuildPlayer connection)
        {
            await connection.DisconnectAsync();
            queue.Clear();
            isLooping = false;
        }

        public static async Task PlaybackFinished(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs e, InteractionContext ctx)
        {
            if (isLooping)
            {
                await sender.PlayAsync(e.Track);
                var eb = new DiscordEmbedBuilder()
                    .WithDescription($"Loop aktiviert: Spielt den aktuellen Titel erneut: \n{SongResolver.GetTrackInfo(e.Track)}")
                    .WithColor(BotConfig.GetEmbedColor())
                    .WithAuthor(e.Track.Info.Title, iconUrl: ctx.Client.CurrentUser.AvatarUrl, url: e.Track.Info.Uri.ToString());
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(eb));
                return;
            }
            else if (queue.Count > 0)
            {
                var nextTrack = queue.Dequeue();
                await sender.PlayAsync(nextTrack);

                int remainingTracks = queue.Count;
                string message = $"Spiele nächsten Titel: {SongResolver.GetTrackInfo(nextTrack)}\n";
                if (remainingTracks > 0)
                {
                    message += $"Verbleibende Songs in der Warteschlange: {remainingTracks}";
                }
                else
                {
                    message += "Keine weiteren Songs in der Warteschlange.";
                }

                var eb = new DiscordEmbedBuilder()
                    .WithDescription(message)
                    .WithColor(BotConfig.GetEmbedColor())
                    .WithAuthor(nextTrack.Info.Title, iconUrl: ctx.Client.CurrentUser.AvatarUrl, url: nextTrack.Info.Uri.ToString());
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(eb));
            }
            else if (sender.CurrentTrack == null)
            {
                await DisconnectAndReset(sender);
            }
        }
    }
}
