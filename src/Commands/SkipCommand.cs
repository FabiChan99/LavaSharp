#region

using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using LavaSharp.Attributes;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;

#endregion

namespace LavaSharp.Commands
{
    public class SkipCommand : ApplicationCommandsModule
    {
        [EnsureGuild]
        [EnsureMatchGuildId]
        [ApplicationRequireExecutorInVoice]
        [RequireRunningPlayer]
        [SlashCommand("skip", "Skips the current song")]
        public async Task Skip(InteractionContext ctx,
            [Option("number_of_tracks", "[Optional] The number of tracks to skip")]
            int tracksToSkip = 1)
        {
            var queue = LavaQueue.queue;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedSessions.First().Value;
            var player = node?.GetGuildPlayer(ctx.Guild);
            var channel = ctx.Member?.VoiceState?.Channel;

            if (player?.Channel.Id != channel?.Id)
            {
                var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel as me.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
                return;
            }

            if (queue.Count == 0)
            {
                var errorEmbed = EmbedGenerator.GetErrorEmbed("There are no songs in the queue.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
                return;
            }

            if (tracksToSkip > queue.Count || tracksToSkip < 1)
            {
                var errorEmbed =
                    EmbedGenerator.GetErrorEmbed(
                        $"Number of tracks to skip must be between 1 and the queue length ({queue.Count}).");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
                return;
            }

            if (tracksToSkip == 1)
            {
                var track = queue.Dequeue();

                CurrentPlayData.track = track.Item1;
                CurrentPlayData.user = track.Item2;
                await player.PlayAsync(track.Item1);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("⏭️ | Skipped the current song."));
                await ctx.Channel.SendMessageAsync(EmbedGenerator.GetCurrentTrackEmbed(track.Item1, player));
                return;
            }
            if (tracksToSkip >= 2)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"⏭️ | Skipping ``{tracksToSkip}`` songs....."));
                LavalinkTrack targettrack = null;
                for (int i = 0; i < tracksToSkip; i++)
                {
                    var track = queue.Dequeue();
                    CurrentPlayData.track = track.Item1;
                    CurrentPlayData.user = track.Item2;
                    targettrack = track.Item1;
                }
                await player.PlayAsync(targettrack);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"⏭️ | Skipped ``{tracksToSkip}`` songs."));
                await ctx.Channel.SendMessageAsync(EmbedGenerator.GetCurrentTrackEmbed(CurrentPlayData.track, player));
                return;
            }

        }
    }   
}