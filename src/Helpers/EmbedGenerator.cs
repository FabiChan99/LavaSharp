using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp.Entities;

namespace LavaSharp.Helpers
{
    public static class EmbedGenerator
    {
        public static DiscordEmbedBuilder GetErrorEmbed(string description)
        {
            return new DiscordEmbedBuilder()
                .WithDescription(description)
                .WithTitle("Error")
                .WithColor(DiscordColor.Red);
        }
    }
}
