using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using LavaSharp.Attributes;
using LavaSharp.Helpers;

namespace LavaSharp.Commands;

public class SendMeTrackCommand : ApplicationCommandsModule
{
    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [RequireRunningPlayer]
    [SlashCommand("sendmetrack", "Sends you the current track.")]
    public static async Task SendMeTrack(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member.VoiceState?.Channel;
        if (player?.Channel.Id != channel?.Id)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel as me.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        if (player?.CurrentTrack == null)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("There is no song playing.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }
        
        // send the track to the user
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(
                $"🔂 | Sending you the current track."));
        // send the track to the user per dm
        var em = new DiscordEmbedBuilder()
            .WithTitle("Current Track")
            .WithDescription($"[{player.CurrentTrack.Info.Title}]({player.CurrentTrack.Info.Uri})")
            .WithColor(DiscordColor.Blurple)
            .WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl)
            .Build();
        bool success = false;
        try {
            await ctx.Member.SendMessageAsync(embed: em);
            success = true;
        } catch (Exception e) {
            Console.WriteLine(e);
        }

        if (!success)
        {
            // follow up message
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .WithContent("❌ | I couldn't send you the track. Please enable DMs from server members."));
        }
        
        
        
        
        
        
        
        
    }
}