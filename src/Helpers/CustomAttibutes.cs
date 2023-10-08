using DisCatSharp.ApplicationCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using LavaSharp.Config;

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
                var embed = new DiscordEmbedBuilder()
                    .WithDescription("You must be in a voice channel to use this command.")
                    .WithColor(BotConfig.GetEmbedColor());
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return false;
            }
            return true;
        }

    }
}
