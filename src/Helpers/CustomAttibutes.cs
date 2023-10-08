using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using LavaSharp.Helpers;

namespace LavaSharp.Attributes
{
    public sealed class ApplicationRequireExecutorInVoice : ApplicationCommandCheckBaseAttribute
    {
        public ApplicationRequireExecutorInVoice()
        {
        }

        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            Console.WriteLine("Executing ApplicationRequireExecutorInVoice");
            if (ctx.Member.VoiceState?.Channel is null)
            {

                var embed = EmbedGenerator.GetErrorEmbed("You must be in a voice channel to use this command.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return false;
            }
            return true;
        }

    }

    public sealed class RequireRunningPlayer : ApplicationCommandCheckBaseAttribute
    {
        public RequireRunningPlayer()
        {
        }

        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            Console.WriteLine("Executing RequireRunningPlayer");
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedSessions.First().Value;
            var player = node.GetGuildPlayer(ctx.Guild);
            if (player is null)
            {
                var errorembed = EmbedGenerator.GetErrorEmbed("I'm not connected to a voice channel.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return false;
            }
            return true;
        }
    }

}
