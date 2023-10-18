using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using LavaSharp.Attributes;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;

namespace LavaSharp.Commands;

public class PauseCommand : ApplicationCommandsModule
{
    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [RequireRunningPlayer]
    [CheckDJ]
    [SlashCommand("pause", "Pauses or resumes the current track.")]
    public static async Task Pause(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member.VoiceState?.Channel;

        if (player?.CurrentTrack == null)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("There is no song playing.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        if (player?.Channel.Id != channel?.Id)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel as me.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        if (LavaQueue.isPaused)
        {
            await player.ResumeAsync();
            LavaQueue.isPaused = false;
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("⏯️️ | Resuming the player."));
            return;
        }

        LavaQueue.isPaused = true;
        await player.PauseAsync();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("⏸️ | Pausing the player."));
    }
}