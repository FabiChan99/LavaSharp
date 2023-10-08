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
        [RequireRunningPlayer]
        [SlashCommand("skip", "Skips the current song")]
        public async Task Skip(InteractionContext ctx)
        {
            var queue = LavaQueue.queue;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedSessions.First().Value;
            var player = node.GetGuildPlayer(ctx.Guild);
            var channel = ctx.Member.VoiceState?.Channel;

            if (player.Channel.Id != channel?.Id)
            {
                var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel as me.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
                return;
            }

            if (queue.Count == 0)
            {
                var errorEmbed = EmbedGenerator.GetErrorEmbed("There are no songs in the queue.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
                return;
            }

            var track = queue.Dequeue();
            await player.PlayAsync(track);

            var playEmbed = EmbedGenerator.GetPlayEmbed(track);
            playEmbed.WithDescription($"Playing next track: {SongResolver.GetTrackInfo(track)}");
            playEmbed.WithTitle("Current Song skipped");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(playEmbed));
        }
    }
}
