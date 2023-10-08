using DisCatSharp.Entities;
using DisCatSharp.Lavalink.Entities;

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

        public static DiscordEmbedBuilder GetPlayEmbed(LavalinkTrack track)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Now Playing")
                .WithDescription($"**{track.Info.Title}**\n*{track.Info.Author}*")
                .WithUrl(track.Info.Uri)
                .WithColor(new DiscordColor(36, 187, 252));

            if (!string.IsNullOrEmpty(track.Info.ArtworkUrl?.ToString()))
            {
                embed.WithThumbnail(track.Info.ArtworkUrl);
            }

            embed.WithFooter("LavaSharp", CurrentApplicationData.BotApplication.AvatarUrl);

            return embed;
        }

        public static DiscordEmbedBuilder GetQueueAddedEmbed(LavalinkTrack track)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Added to Queue")
                .WithDescription($"**{track.Info.Title}**\n*{track.Info.Author}* - {track.Info.Length}")
                .WithUrl(track.Info.Uri)
                .WithColor(new DiscordColor(36, 187, 252));
            if (!string.IsNullOrEmpty(track.Info.ArtworkUrl?.ToString()))
            {
                embed.WithThumbnail(track.Info.ArtworkUrl);
            }

            embed.WithFooter("LavaSharp", CurrentApplicationData.BotApplication.AvatarUrl);

            return embed;
        }
    }
}
