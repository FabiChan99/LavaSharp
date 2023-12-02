using System.Globalization;
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
using DisCatSharp.Lavalink.Enums;
using LavaSharp.Attributes;
using LavaSharp.Config;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;
using Microsoft.Extensions.Logging;

namespace LavaSharp.Commands;

[SlashCommandGroup("queue", "Queue commands.")]
public class QueueCommand : ApplicationCommandsModule
{
    static bool importIsCancelled = false;
    static bool importisrunning = false;
    
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



    [EnsureGuild]
    [EnsureMatchGuildId]
    [RequireRunningPlayer]
    [ApplicationRequireExecutorInVoice]
    [CheckDJ]
    [SlashCommand("export", "Exports the current queue to a file to re-import it later.")]
    public static async Task ExportQueue(InteractionContext ctx)
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
        
        if (LavaQueue.queue.Count == 0)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("There are no songs in the queue.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("📥 | Exporting the queue..."));
        
                
        var queueList = LavaQueue.queue.ToList();
        var tracks = new List<LavalinkTrack>();
        foreach (var item in queueList)
        {
            tracks.Add(item.Item1);
        }
        
        var csv = new StringBuilder();
        csv.AppendLine("\"Title\",\"Author\",\"Length\",\"Url\"");
        foreach (var item in tracks)
        {
            var newLine = $"\"{item.Info.Title}\",\"{item.Info.Author}\",\"{item.Info.Length}\",\"{item.Info.Uri}\"";
            csv.AppendLine(newLine);
        }

        
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        using var stream = new MemoryStream(bytes);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var updatedMessageWithFile = new DiscordWebhookBuilder()
            .WithContent("📥 | Exported the queue.").AddFile($"queue-{timestamp}.lsq", stream);
        await ctx.EditResponseAsync(updatedMessageWithFile);
    }

    [EnsureGuild]
    [EnsureMatchGuildId]

    [ApplicationRequireExecutorInVoice]
    [CheckDJ]
    [SlashCommand("import", "Imports a queue from a file.")]
    public static async Task ImportQueue(InteractionContext ctx,
        [Option("file", "The file to import.")]
        DiscordAttachment queuefile)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member.VoiceState?.Channel;

        if (player != null && player.Channel.Id != channel?.Id)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel as me.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }
        
        bool wasinit = true;
        
        if (player is null)
        {
            wasinit = false;
            player = await node.ConnectAsync(channel);
            PlayCommand.RegisterPlaybackFinishedEvent(player, ctx);
        }
        
        
        if (player?.CurrentTrack != null)
        {
            var eb = new DiscordEmbedBuilder();
            eb.WithTitle("Queueimporter");
            eb.WithDescription(
                $"Are you sure you want to import the queue from the file ``{queuefile.FileName}``? This will clear the current queue.");
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
                    new DiscordWebhookBuilder().WithContent("⏱️ | Timed out - Queueimport cancelled."));
                return;
            }

            if (result.Result.Id == "no")
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("🚫 | Cancelled."));
                return;
            }
        }
        
        if (!wasinit)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("📥 | Importing the queue..."));
        }
        
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("📥 | Importing the queue... This can take a while"));

        var queueList = new List<(LavalinkTrack, DiscordUser)>();

        var url = queuefile.Url;
        using var client = new HttpClient();
        await using var fileStream = await client.GetStreamAsync(url);
        var urlList = ReadUrlsFromCsv(fileStream);

        var i = 1;
        var total = urlList.Count;
        bool CancelationInvoked = false;
        importisrunning = true;

        foreach (var item in urlList)
        {
            try
            {
                if (importIsCancelled)
                {
                    importIsCancelled = false;
                    CancelationInvoked = true;
                    break;
                }
                var query = item;
                var loadResult = await player.LoadTracksAsync(LavalinkSearchType.Plain, query);
                if (loadResult.LoadType == LavalinkLoadResultType.Track)
                {
                    queueList.Add((loadResult.GetResultAs<LavalinkTrack>(), ctx.User));
                }

                if (i % 15 == 0)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"🔄 | {i} of {total} songs have been imported... importing can take a while...."));
                }

            }
            catch (Exception e)
            {
                ctx.Client.Logger.LogError(e, "Error while importing a song.");
            }
            i++;
        }

        importisrunning = false;
        if (CancelationInvoked)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"🚫 | Queue import cancelled. {i} of {total} songs have been imported."));
        }
        
        if (!CancelationInvoked)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("🔄 | Importing the queue..."));
        }
        
        LavaQueue.queue = new Queue<(LavalinkTrack, DiscordUser)>(queueList);
        
        var volstr = $"📥 | Imported the queue from the file ``{queuefile.FileName}``.";
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(volstr));
        
        if (player?.CurrentTrack == null)
        {
            var queueTrack = LavaQueue.queue.Dequeue();
            await player.PlayAsync(queueTrack.Item1);
            CurrentPlayData.track = queueTrack.Item1;
            CurrentPlayData.user = ctx.User;
            CurrentPlayData.CurrentExecutionChannel = ctx.Channel;
            await NowPlaying.sendNowPlayingTrack(ctx, queueTrack.Item1);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"🎵 | Now playing ``{queueTrack.Item1.Info.Title}``"));
        }
    }
    
    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [CheckDJ]
    [SlashCommand("cancelimport", "Cancels the import of a queue.")]
    public static async Task CancelImport(InteractionContext ctx)
    {
        if (importisrunning)
        {
            importIsCancelled = true;
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("🚫 | Cancelled the Queueimport."));
        }
        else
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("❌ | There is no running import to cancel."));
        }
    }
    
    
    
    
    
    
    
    
    private static List<string> ReadUrlsFromCsv(Stream fileStream)
    {
        var urlList = new List<string>();
        using var csv = new StreamReader(fileStream);

        while (!csv.EndOfStream)
        {
            var line = csv.ReadLine();
            var values = line?.Split(',');
            if (values != null && values.Length > 3)
            {
                urlList.Add(values[3]);
            }
        }

        return urlList;
    }

    
    
    
}