using System.Runtime.InteropServices;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using LavaSharp.Config;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;
using LavaSharp.Attributes;

namespace LavaSharp.Commands
{
    public class SkipCommand : ApplicationCommandsModule
    {
        [ApplicationRequireExecutorInVoice]
        [SlashCommand("skip", "Skips the current song")]
        public async Task Skip(InteractionContext ctx)
        {
            // Get queue from LavaQueue
            var queue = LavaQueue.queue;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedSessions.First().Value;
            var player = node.GetGuildPlayer(ctx.Guild);
            var channel = ctx.Member.VoiceState?.Channel;

            if (player is null)
            {
                // If player is null, bot is not connected to voice
                var embed = new DiscordEmbedBuilder()
                    .WithDescription("I'm not connected to a voice channel.")
                    .WithColor(BotConfig.GetEmbedColor());

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            if (player.Channel.Id != channel?.Id)
            {
                // If player is not in the same channel as the user
                var embed = new DiscordEmbedBuilder()
                    .WithDescription("You must be in the same voice channel as me.")
                    .WithColor(BotConfig.GetEmbedColor());

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            // Skip logic

            if (channel is null)
            {
                // If user is not in a voice channel
                var embed = new DiscordEmbedBuilder()
                    .WithDescription("You must be in a voice channel to use this command.")
                    .WithColor(BotConfig.GetEmbedColor());

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            if (queue.Count == 0)
            {
                // If queue is empty
                var embed = new DiscordEmbedBuilder()
                    .WithDescription("There are no songs in the queue.")
                    .WithColor(BotConfig.GetEmbedColor());

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            // If queue count is greater than 0
            var track = queue.Dequeue();
            await player.PlayAsync(track);

            var embed2 = new DiscordEmbedBuilder()
                .WithDescription($"Playing next track: {SongResolver.GetTrackInfo(track)}")
                .WithColor(BotConfig.GetEmbedColor())
                .WithAuthor(track.Info.Title, iconUrl: ctx.Client.CurrentUser.AvatarUrl, url: track.Info.Uri.ToString());

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed2));
        }
    }
}
