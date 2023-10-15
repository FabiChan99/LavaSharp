using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using LavaSharp.Attributes;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;

namespace LavaSharp.Commands;

public class ShuffleCommand : ApplicationCommandsModule
{
    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [RequireRunningPlayer]
    [CheckDJ]
    [SlashCommand("shuffle", "Randomizes the current queue")]
    public static async Task Shuffle(InteractionContext ctx)
    {
        var queue = LavaQueue.queue;
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node?.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member?.VoiceState?.Channel;

        if (player?.Channel.Id != channel?.Id)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel as me.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }
        if (queue.Count < 2)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("There are not enough songs in the queue to shuffle.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }
        try
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("🔀 | Queue shuffeling in progress..."));
            var newQueue = queue.OrderBy(a => Guid.NewGuid()).ToList();
            queue.Clear();
            foreach (var item in newQueue)
            {
                queue.Enqueue(item);
            }
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("🔀 | Queue shuffeled!"));
        }
        catch (Exception e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"🔀 | An error occured while randomizing the Queue:```{e.Message}```"));
        }
    }
}