using DisCatSharp.ApplicationCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands.Context;

namespace LavaSharp.Attributes
{
    public sealed class ApplicationRequireExecutorInVoice : ApplicationCommandCheckBaseAttribute
    {
        public ApplicationRequireExecutorInVoice()
        {
        }

        public override Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            return Task.FromResult(ctx.Member.VoiceState?.Channel is not null);
        }

    }
}
