using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using LavaSharp.Attributes;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;

namespace LavaSharp.Commands
{
    public class StopCommand : ApplicationCommandsModule
    {
        [EnsureGuild]
        [EnsureMatchGuildId]
        [ApplicationRequireExecutorInVoice]
        [RequireRunningPlayer]
        [SlashCommand("stop", "Stops the current song")]
        public async Task Stop(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedSessions.First().Value;
            var player = node.GetGuildPlayer(ctx.Guild);
            if (player.Channel != ctx.Member?.VoiceState?.Channel)
            {
                var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel as me.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
                return;
            }
            await LavaQueue.DisconnectAndReset(player);
            var StopEmbed = new DiscordEmbedBuilder();
            StopEmbed.WithDescription($"Player Destroyed.");
            StopEmbed.WithTitle("The Player has been stopped.").WithColor(DiscordColor.Red);
            StopEmbed.WithFooter(ctx.User.UsernameWithDiscriminator);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(StopEmbed));
        }
    }
}
