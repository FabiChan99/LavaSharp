#region

using System.Text.RegularExpressions;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using LavaSharp.Attributes;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;

#endregion

namespace LavaSharp.Commands;

public class PlayCommand : ApplicationCommandsModule
{
    public enum LavaSourceType
    {
        AutoDetect,
        YouTube,
        Spotify,
        SoundCloud
    }

    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [SlashCommand("play", "Plays a song.")]
    public static async Task Play(InteractionContext ctx,
        [Option("query", "The query to search for (URL or Song Name)")]
        string query,
        [Option("sourcetype", "The Search source. For Links use Auto-Detect or Link")] LavaSourceType sourceType =
            LavaSourceType.AutoDetect)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member?.VoiceState?.Channel;
        LavalinkGuildPlayer? lavaPlayer = null;
        if (player != null && player.Channel != channel)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel!");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        if (player is null)
        {
            lavaPlayer = await node.ConnectAsync(channel);
        }
        else
        {
            lavaPlayer = player;
        }

        if (lavaPlayer?.Player is null)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("Failed to connect to voice channel!");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent($"🔎 | Resolving ``{query}``..."));

        query = FilterQueryIfYoutube(query);
        var loadResult = await lavaPlayer.LoadTracksAsync(ResolveSongType(query, sourceType), query);
        LavalinkTrack track = new LavalinkTrack();
        List<LavalinkTrack> tracks = new List<LavalinkTrack>();
        var loadType = loadResult.LoadType;
        try
        {
            if (loadResult.LoadType == LavalinkLoadResultType.Track)
            {
                track = loadResult.GetResultAs<LavalinkTrack>();
            }
            else if (loadResult.LoadType == LavalinkLoadResultType.Playlist)
            {
                tracks = loadResult.GetResultAs<LavalinkPlaylist>().Tracks;
            }
            else if (loadResult.LoadType == LavalinkLoadResultType.Search)
            {
                var searchResult = loadResult.GetResultAs<List<LavalinkTrack>>();
                var searchResultString = "";
                var resultstrings = new List<string>();
                if (sourceType == LavaSourceType.Spotify)
                {
                    for (int i = 0; i < Math.Min(searchResult.Count, 25); i++)
                    {
                        searchResultString +=
                            $"**{i + 1}**. [{searchResult[i].Info.Author} - {searchResult[i].Info.Title}]({searchResult[i].Info.Uri}) ``{searchResult[i].Info.Length}``\n";
                        resultstrings.Add(
                            $"**{i + 1}**. {searchResult[i].Info.Author} - {searchResult[i].Info.Title} -- {searchResult[i].Info.Length}");
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Min(searchResult.Count, 25); i++)
                    {
                        searchResultString +=
                            $"**{i + 1}**. [{searchResult[i].Info.Title}]({searchResult[i].Info.Uri}) ``{searchResult[i].Info.Length}``\n";
                        resultstrings.Add($"{i + 1}. {searchResult[i].Info.Title} -- {searchResult[i].Info.Length}");
                    }
                }

                var maxOptionLength = 95;
                var truncatedResultStrings = resultstrings
                    .Select(s => s.Length > maxOptionLength ? s.Substring(0, maxOptionLength) : s).ToList();

                var options = new List<DiscordStringSelectComponentOption>();
                for (int i = 0; i < resultstrings.Count; i++)
                {
                    options.Add(new DiscordStringSelectComponentOption(truncatedResultStrings[i], i.ToString()));
                }

                var generatedId = Guid.NewGuid().ToString();
                var select = new DiscordStringSelectComponent("Select a song", options, generatedId);
                var embed = new DiscordEmbedBuilder();
                embed.WithTitle("Search Results");
                embed.WithDescription(searchResultString);
                embed.WithColor(DiscordColor.Blurple);
                embed.WithFooter($"Requested by {ctx.User.Username}#{ctx.User.Discriminator}");
                var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed)
                    .AddComponents(select));
                var interactivity = ctx.Client.GetInteractivity();
                var result = await interactivity.WaitForSelectAsync(msg, ctx.User, generatedId,
                    ComponentType.StringSelect, TimeSpan.FromMinutes(2));
                if (result.TimedOut)
                {
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().WithContent("⚠️ | Timed out! No song selected."));
                    return;
                }

                var selectedOption = result.Result.Values.First();
                var selectedTrack = searchResult[int.Parse(selectedOption)];
                track = selectedTrack;
            }
            else if (loadResult.LoadType == LavalinkLoadResultType.Error)
            {
                throw new InvalidOperationException($"Error loading track: {loadResult}");
            }
            else if (loadResult.LoadType == LavalinkLoadResultType.Empty)
            {
                throw new FileNotFoundException("No Results");
            }
            else
            {
                throw new InvalidOperationException("Unexpected load result type.");
            }
        }
        catch (FileNotFoundException)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("\u26a0\ufe0f | No results found!"));
            return;
        }
        catch (InvalidOperationException)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("\u274c | An error occoured!"));
            return;
        }

        bool isPlaying = lavaPlayer.CurrentTrack is not null;
        if (isPlaying && loadType == LavalinkLoadResultType.Track ||
            isPlaying && loadType == LavalinkLoadResultType.Search)
        {
            LavaQueue.queue.Enqueue((track, ctx.User));
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"🎵 | Added **{track.Info.Title}** to the queue."));
            return;
        }

        if (isPlaying && loadType == LavalinkLoadResultType.Playlist)
        {
            foreach (var item in tracks)
            {
                LavaQueue.queue.Enqueue((item, ctx.User));
            }

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"🎵 | Added **{tracks.Count}** tracks to the queue."));
            return;
        }

        RegisterPlaybackFinishedEvent(lavaPlayer, ctx);
        if (loadType == LavalinkLoadResultType.Track || loadType == LavalinkLoadResultType.Search)
        {
            CurrentPlayData.track = track;
            CurrentPlayData.user = ctx.User;
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"🎵 | Added **{track.Info.Title}** to the queue."));
            await ctx.Channel.SendMessageAsync(EmbedGenerator.GetCurrentTrackEmbed(track, lavaPlayer));
            await lavaPlayer.PlayAsync(track);
            return;
        }

        if (loadType == LavalinkLoadResultType.Playlist)
        {
            CurrentPlayData.track = tracks.First();
            CurrentPlayData.user = ctx.User;
            foreach (var item in tracks)
            {
                LavaQueue.queue.Enqueue((item, ctx.User));
            }

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"🎵 | Added **{tracks.Count}** tracks to the queue."));

            await ctx.Channel.SendMessageAsync(EmbedGenerator.GetCurrentTrackEmbed(tracks.First(), lavaPlayer));
            await lavaPlayer.PlayAsync(tracks.First());

            LavaQueue.queue.Dequeue();
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ | An error occoured!"));
    }


    private static void RegisterPlaybackFinishedEvent(LavalinkGuildPlayer player, InteractionContext ctx)
    {
        if (player?.CurrentTrack is null)
        {
            return;
        }

        player.TrackEnded += (sender, e) => LavaQueue.PlaybackFinished(sender, e, ctx);
    }

    private static LavalinkSearchType ResolveSongType(string query, LavaSourceType userSourceType)
    {
        if (userSourceType == LavaSourceType.AutoDetect)
        {
            if (RegexTemplates.YouTubeUrl.IsMatch(query))
            {
                return LavalinkSearchType.Plain;
            }

            if (RegexTemplates.SpotifyUrl.IsMatch(query))
            {
                return LavalinkSearchType.Plain;
            }

            if (RegexTemplates.SoundcloudUrl.IsMatch(query))
            {
                return LavalinkSearchType.Plain;
            }

            if (RegexTemplates.Url.IsMatch(query))
            {
                return LavalinkSearchType.Plain;
            }

            return LavalinkSearchType.Youtube;
        }

        return SearchType(userSourceType);
    }

    private static string FilterQueryIfYoutube(string query)
    {
        if (RegexTemplates.YouTubeUrl.IsMatch(query))
        {
            if (Regex.IsMatch(query, @"((\?|&)list=RDMM\w+)(&*)"))
            {
                Group group = Regex.Match(query, @"((\?|&)list=RDMM\w+)(&*)", RegexOptions.ExplicitCapture);
                var value = group.Value;

                if (value.EndsWith("&"))
                    value = value[..^1];

                query = query.Replace(value, "");
            }

            if (Regex.IsMatch(query, @"((\?|&)start_radio=\d+)(&*)"))
            {
                Group group = Regex.Match(query, @"((\?|&)start_radio=\d+)(&*)", RegexOptions.ExplicitCapture);
                var value = group.Value;

                if (value.EndsWith("&"))
                    value = value[..^1];

                query = query.Replace(value, "");
            }

            var AndIndex = query.IndexOf("&");

            if (!query.Contains('?') && AndIndex != -1)
            {
                query = query.Remove(AndIndex, 1);
                query = query.Insert(AndIndex, "?");
            }
        }
        else
        {
            return query;
        }

        return query;
    }


    private static LavalinkSearchType SearchType(LavaSourceType sourceType)
    {
        switch (sourceType)
        {
            case LavaSourceType.YouTube:
                return LavalinkSearchType.Youtube;
            case LavaSourceType.Spotify:
                return LavalinkSearchType.Spotify;
            case LavaSourceType.SoundCloud:
                return LavalinkSearchType.SoundCloud;
            default:
                return LavalinkSearchType.Youtube;
        }
    }
}