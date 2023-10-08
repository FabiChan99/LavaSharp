using DisCatSharp.ApplicationCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using LavaSharp.Config;

namespace LavaSharp.Attributes
{
    public sealed class ApplicationRequireExecutorInVoice : ApplicationCommandCheckBaseAttribute
    {
        public ApplicationRequireExecutorInVoice()
        {
        }

        public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            if (ctx.Member.VoiceState?.Channel is null)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithDescription("You must be in a voice channel to use this command.")
                    .WithColor(BotConfig.GetEmbedColor());
            }

            return Task.FromResult(ctx.Member.VoiceState?.Channel is not null);
        }

    }
}
