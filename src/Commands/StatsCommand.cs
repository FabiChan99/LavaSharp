using System.Diagnostics;
using System.Reflection;
using System.Text;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using LavaSharp.Attributes;
using LavaSharp.Config;
using LavaSharp.Helpers;

namespace LavaSharp.Commands;

public class StatsCommand : ApplicationCommandsModule
{
    [EnsureGuild]
    [EnsureMatchGuildId]
    [SlashCommand("stats", "Shows the stats of the bot.")]
    public static async Task Stats(InteractionContext ctx)
    {
        var osversion = Environment.OSVersion.VersionString;
        var NetVersion = Environment.Version.ToString();
        var botuptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
        var botuptimeformatted = $"{botuptime.Days} days, {botuptime.Hours} hours, {botuptime.Minutes} minutes, {botuptime.Seconds} seconds";
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var lavauptime = node.Statistics.Uptime;
        var lavauptimeformatted = $"{lavauptime.Days} days, {lavauptime.Hours} hours, {lavauptime.Minutes} minutes, {lavauptime.Seconds} seconds";
        var lavaram = node.Statistics.Memory.Allocated / 1024 / 1024;
        var lavaramused = node.Statistics.Memory.Used / 1024 / 1024;
        var sysuptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var sysuptimeformatted = $"{sysuptime.Days} days, {sysuptime.Hours} hours, {sysuptime.Minutes} minutes, {sysuptime.Seconds} seconds";
        DiscordEmbedBuilder eb = new DiscordEmbedBuilder();
        eb.WithTitle($"{ctx.Client.CurrentUser.Username} Information");
        var desc = new StringBuilder();
        desc.AppendLine($"**Bot Uptime:** {botuptimeformatted}");
        desc.AppendLine($"**Lavalink Uptime:** {lavauptimeformatted}");
        desc.AppendLine($"**System Uptime:** {sysuptimeformatted}");
        desc.AppendLine($"**Lavalink RAM Usage:** {lavaramused}MB/{lavaram}MB");
        desc.AppendLine($"**OS Version:** {osversion}");
        desc.AppendLine($"**.NET Runtime Version:** {NetVersion}");
        eb.WithDescription(desc.ToString());
        eb.WithColor(BotConfig.GetEmbedColor());
        eb.WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl);
        eb.WithTimestamp(DateTime.Now);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(eb));
    }
}