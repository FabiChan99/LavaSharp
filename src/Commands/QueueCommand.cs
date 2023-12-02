using System.Text;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using LavaSharp.Attributes;
using LavaSharp.Config;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;

namespace LavaSharp.Commands;

[SlashCommandGroup("queue", "Queue commands.")]
public class QueueCommand : ApplicationCommandsModule
{
    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [SlashCommand("current", "Shows the current queue.")]
    public static async Task Loop(InteractionContext ctx)
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


        var queue = LavaQueue.queue;

        if (queue.Count == 0)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("There are no songs in the queue.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        // acknowledge the interaction

        // Paginate the queue
        var pages = new List<Page>();
        var queueList = queue.ToList();
        var queueString = new StringBuilder();
        var i = 1;

        List<LavalinkTrack> tracks = new();
        foreach (var item in queue)
        {
            tracks.Add(item.Item1);
        }

        // get the total length of the queue
        var totalTime = new TimeSpan();

        foreach (var item in tracks)
        {
            totalTime += item.Info.Length;
        }


        string formattedTime =
            $"{(int)totalTime.TotalDays} Days, {totalTime.Hours} Hours, {totalTime.Minutes} Minutes, {totalTime.Seconds} Seconds";

        foreach (var item in queueList)
        {
            queueString.AppendLine(
                $"``{i}.`` {item.Item1.Info.Author} - {item.Item1.Info.Title} ({item.Item1.Info.Length:hh\\:mm\\:ss}) | Requested by {item.Item2.Mention}");
            i++;
            if (i % 25 == 0)
            {
                var eb = new DiscordEmbedBuilder();
                eb.WithTitle("Queue");
                eb.WithDescription(queueString.ToString());
                eb.WithColor(BotConfig.GetEmbedColor());
                eb.WithFooter(
                    $"Requested by {ctx.Member.DisplayName} | Page {pages.Count + 1} of {Math.Ceiling((double)queue.Count / 25)}",
                    ctx.Member.AvatarUrl);
                eb.WithTimestamp(DateTime.Now);
                pages.Add(new Page(embed: eb,
                    content:
                    $"🎵 | Showing the queue. There are {queue.Count} songs in the queue with a total length of ``{formattedTime}``."));
                queueString.Clear();
            }
        }

        // Add the last page if there are remaining songs
        if (queueString.Length > 0)
        {
            var eb = new DiscordEmbedBuilder();
            eb.WithTitle("Queue");
            eb.WithDescription(queueString.ToString());
            eb.WithColor(BotConfig.GetEmbedColor());
            eb.WithFooter($"Requested by {ctx.Member.DisplayName} | Page {pages.Count + 1}", ctx.Member.AvatarUrl);
            eb.WithTimestamp(DateTime.Now);
            pages.Add(new Page(embed: eb,
                content:
                $"🎵 | Showing the queue. There are {queue.Count} songs in the queue with a total length of ``{formattedTime}``."));
        }

        await ctx.Interaction.SendPaginatedResponseAsync(false, false, ctx.User, pages);
    }

    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [CheckDJ]
    [SlashCommand("clear", "Clears the queue.")]
    public static async Task ClearQueue(InteractionContext ctx)
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

        if (LavaQueue.queue.Count == 0)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("There are no songs in the queue.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        LavaQueue.queue.Clear();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("🗑️ | Cleared the queue."));
    }

    [EnsureGuild]
    [EnsureMatchGuildId]
    [RequireRunningPlayer]
    [ApplicationRequireExecutorInVoice]
    [CheckDJ]
    [SlashCommand("removesong",
        "Removes a song from the queue. (Get the song number from the '/queue current' command)")]
    public static async Task RemoveSong(InteractionContext ctx,
        [Option("songnumber", "The song number to remove.")]
        int songnumber)
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

        if (songnumber > LavaQueue.queue.Count || songnumber < 1)
        {
            var errorEmbed =
                EmbedGenerator.GetErrorEmbed(
                    $"Song number must be between 1 and the queue length ({LavaQueue.queue.Count}).");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        var queueList = LavaQueue.queue.ToList();
        var tracktoRemove = queueList[songnumber - 1];

        var eb = new DiscordEmbedBuilder();
        eb.WithTitle("Songremover");
        eb.WithDescription($"Are you sure you want to remove ``{tracktoRemove.Item1.Info.Title}`` from the queue?");
        eb.WithColor(BotConfig.GetEmbedColor());
        eb.WithFooter($"{ctx.Member.UsernameWithDiscriminator}", ctx.Member.AvatarUrl);
        eb.WithTimestamp(DateTime.Now);
        var buttons = new List<DiscordButtonComponent>();
        var yesButton = new DiscordButtonComponent(ButtonStyle.Success, "yes", "Yes");
        var noButton = new DiscordButtonComponent(ButtonStyle.Danger, "no", "No");
        buttons.Add(yesButton);
        buttons.Add(noButton);
        var irb = new DiscordInteractionResponseBuilder();
        irb.AddEmbed(eb);
        irb.AddComponents(buttons);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, irb);
        var interactivity = ctx.Client.GetInteractivity();

        bool MatchAuthor(ComponentInteractionCreateEventArgs args)
        {
            return args.User.Id == ctx.User.Id;
        }

        var result = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), MatchAuthor,
            TimeSpan.FromSeconds(45));
        if (result.TimedOut)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("⏱️ | Timed out - Songremoval cancelled."));
            return;
        }

        if (result.Result.Id == "no")
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("🚫 | Cancelled."));
            return;
        }

        if (queueList[songnumber - 1].Item1 != tracktoRemove.Item1)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("🚫 | The queue has changed. Please try again."));
            return;
        }

        queueList.RemoveAt(songnumber - 1);
        LavaQueue.queue = new Queue<(LavalinkTrack, DiscordUser)>(queueList);
        var volstr = $"🗑️ | Removed song number ``{songnumber}`` from the queue.";
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(volstr));
    }

    [EnsureGuild]
    [EnsureMatchGuildId]
    [RequireRunningPlayer]
    [ApplicationRequireExecutorInVoice]
    [CheckDJ]
    [SlashCommand("moveentry",
        "Moves a song in the queue to a other pos. (Get the song number from the '/queue current' command)")]
    public static async Task MoveQueueEntry(InteractionContext ctx,
        [Option("songnumber", "The song number to move.")]
        int songnumber,
        [Option("newposition", "The new position of the song.")]
        int newposition)
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

        if (songnumber > LavaQueue.queue.Count || songnumber < 1)
        {
            var errorEmbed =
                EmbedGenerator.GetErrorEmbed(
                    $"Song number must be between 1 and the queue length ({LavaQueue.queue.Count}).");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        if (newposition > LavaQueue.queue.Count || newposition < 1)
        {
            var errorEmbed =
                EmbedGenerator.GetErrorEmbed(
                    $"New position must be between 1 and the queue length ({LavaQueue.queue.Count}).");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        var queueList = LavaQueue.queue.ToList();
        var tracktoMove = queueList[songnumber - 1];

        var eb = new DiscordEmbedBuilder();
        eb.WithTitle("Songmover");
        eb.WithDescription(
            $"Are you sure you want to move ``{tracktoMove.Item1.Info.Title}`` from position ``{songnumber}`` to position ``{newposition}``?");
        eb.WithColor(BotConfig.GetEmbedColor());
        eb.WithFooter($"{ctx.Member.UsernameWithDiscriminator}", ctx.Member.AvatarUrl);
        eb.WithTimestamp(DateTime.Now);
        var buttons = new List<DiscordButtonComponent>();
        var yesButton = new DiscordButtonComponent(ButtonStyle.Success, "yes", "Yes");
        var noButton = new DiscordButtonComponent(ButtonStyle.Danger, "no", "No");
        buttons.Add(yesButton);
        buttons.Add(noButton);
        var irb = new DiscordInteractionResponseBuilder();
        irb.AddEmbed(eb);
        irb.AddComponents(buttons);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, irb);
        var interactivity = ctx.Client.GetInteractivity();

        bool MatchAuthor(ComponentInteractionCreateEventArgs args)
        {
            return args.User.Id == ctx.User.Id;
        }

        var result = await interactivity.WaitForButtonAsync(await ctx.GetOriginalResponseAsync(), MatchAuthor,
            TimeSpan.FromSeconds(45));
        if (result.TimedOut)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("⏱️ | Timed out - Songmove cancelled."));
            return;
        }

        if (result.Result.Id == "no")
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("🚫 | Cancelled."));
            return;
        }

        if (queueList[songnumber - 1].Item1 != tracktoMove.Item1)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("🚫 | The queue has changed. Please try again."));
            return;
        }

        queueList.RemoveAt(songnumber - 1);
        queueList.Insert(newposition - 1, tracktoMove);
        LavaQueue.queue = new Queue<(LavalinkTrack, DiscordUser)>(queueList);
        var volstr = $"🗑️ | Moved song number ``{songnumber}`` to position ``{newposition}``.";
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(volstr));
        return;
    }
}