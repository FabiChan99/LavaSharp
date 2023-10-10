using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using LavaSharp.Attributes;
using LavaSharp.Helpers;

namespace LavaSharp.Commands;

public class SeekCommand : ApplicationCommandsModule
{
    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [RequireRunningPlayer]
    [SlashCommand("seek", "Seeks to a certain time in the song")]
    public async Task Seek(InteractionContext ctx, [Option("time", "The time to seek to")] string timeString)
    {
        if (!TimeParser.TryParseTime(timeString, out TimeSpan time))
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("Invalid time format. Please use format mm:ss.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member.VoiceState?.Channel;

        if (player.Channel.Id != channel?.Id)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel as me.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        if (player.CurrentTrack == null)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("There is no song playing.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        if (time > player.CurrentTrack.Info.Length)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("You cannot seek past the end of the song.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        await player.SeekAsync(time);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Seeked to {time.ToString(@"mm\:ss")}"));
    }
}