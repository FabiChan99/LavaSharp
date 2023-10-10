using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using LavaSharp.Config;
using LavaSharp.LavaManager;

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
        public static DiscordEmbedBuilder GetCurrentTrackEmbed(LavalinkTrack track, LavalinkGuildPlayer player)
        {
            var requester = CurrentPlayData.user;
            var queuelength = LavaQueue.queue.Count;
            string eburl = string.Empty;
            try
            {
                eburl = BotConfig.GetConfig()["EmbedConfig"]["EmbedImageURL"];
            }
            catch (Exception)
            {
                eburl = string.Empty;
            }
            var eb = new DiscordEmbedBuilder()
                .WithTitle("Now Playing")
                .WithDescription("**" + track.Info.Title + "**")
                .AddField(new DiscordEmbedField("Duration", $"``{track.Info.Length.ToString()}``", true))
                .AddField(new DiscordEmbedField("Queue", $"{queuelength.ToString()}", true))
                .AddField(new DiscordEmbedField("Volume", $"``{CurrentPlayData.CurrentVolume.ToString() + "%"}``", true))
                .AddField(new DiscordEmbedField("Requested by", requester.Mention, true))
                .WithColor(BotConfig.GetEmbedColor());
            if (!string.IsNullOrEmpty(eburl))
            {
                if (!Uri.IsWellFormedUriString(eburl, UriKind.Absolute))
                {
                    eburl = string.Empty;
                }
                else if (Uri.IsWellFormedUriString(eburl, UriKind.Absolute))
                {
                    eb.WithThumbnail(eburl);
                }
            }
            return eb;
        }

    }
}
