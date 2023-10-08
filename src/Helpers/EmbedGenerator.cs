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
                .WithFooter("Oops! Something went wrong.", "https://cdn.discordapp.com/emojis/755048875965939833.webp")
                .WithColor(new DiscordColor(255, 69, 58)); 
        }

    }
}
