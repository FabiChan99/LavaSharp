using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using LavaSharp.Attributes;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;

namespace LavaSharp.Commands;



public class PlayCommand : ApplicationCommandsModule
{
    [EnsureGuild]
    [EnsureMatchGuildId]
    [ApplicationRequireExecutorInVoice]
    [SlashCommand("play", "Plays a song.")]
    public static async Task Play(InteractionContext ctx,
        [Option("query", "The query to search for (URL or Song Name)")] string query, [Option("sourcetype", "The Search source. For Links use Auto-Detect or Link")] LavaSourceType sourceType = LavaSourceType.AutoDetect)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member?.VoiceState?.Channel;
        LavalinkGuildPlayer? lavaPlayer = null;
        if (player != null && player.Channel != channel)
        {
            var errorEmbed = EmbedGenerator.GetErrorEmbed("You must be in the same voice channel!");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorEmbed));
            return;
        }
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
                track = loadResult.GetResultAs<List<LavalinkTrack>>().First();
                // TODO: Add a way to select a track from the search results.
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("⚠️ | No results found!"));
            return;
        }
        catch (InvalidOperationException)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent("❌ | An error occoured!"));
            return;
        }
        bool isPlaying = lavaPlayer.CurrentTrack is not null;
        if (isPlaying && loadType == LavalinkLoadResultType.Track || loadType == LavalinkLoadResultType.Search)
        {
            LavaQueue.queue.Enqueue((track, ctx.User));
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"🎵 | Added **{track.Info.Title}** to the queue."));
            return;
        }
        else if (isPlaying && loadType == LavalinkLoadResultType.Playlist)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"🎵 | Added **{tracks.Count}** tracks to the queue."));
            foreach (var item in tracks)
            {
                LavaQueue.queue.Enqueue((item, ctx.User));
            }

            var originalmsg = await ctx.GetOriginalResponseAsync();
            await originalmsg.ModifyAsync($"🎵 | Added **{tracks.Count}** tracks to the queue.");
            return;
        }


        else
        {
            RegisterPlaybackFinishedEvent(lavaPlayer, ctx);
            if (loadType == LavalinkLoadResultType.Track || loadType == LavalinkLoadResultType.Search)
            {
                CurrentPlayData.track = track;
                CurrentPlayData.user = ctx.User;
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"🎵 | Added **{track.Info.Title}** to the queue."));
                await ctx.Channel.SendMessageAsync(embed: EmbedGenerator.GetCurrentTrackEmbed(track, lavaPlayer));
                await lavaPlayer.PlayAsync(track);
                return;
            }
            else if (loadType == LavalinkLoadResultType.Playlist)
            {
                // same as above but with a playlist
                CurrentPlayData.track = tracks.First();
                CurrentPlayData.user = ctx.User;
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"🎵 | Added **{tracks.Count}** tracks to the queue."));
                foreach (var item in tracks)
                {
                    LavaQueue.queue.Enqueue((item, ctx.User));
                }

                var originalmsg = await ctx.GetOriginalResponseAsync();
                await originalmsg.ModifyAsync($"🎵 | Added **{tracks.Count}** tracks to the queue.");
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                       new DiscordInteractionResponseBuilder().WithContent("❌ | An error occoured!"));
                return;
            }
        }

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
            else if (RegexTemplates.SpotifyUrl.IsMatch(query))
            {
                return LavalinkSearchType.Plain;
            }
            else if (RegexTemplates.SoundcloudUrl.IsMatch(query))
            {
                return LavalinkSearchType.Plain;
            }
            else if (RegexTemplates.Url.IsMatch(query))
            {
                return LavalinkSearchType.Plain;
            }
            else
            {
                return LavalinkSearchType.Youtube;
            }
        }
        else
        {
            return SearchType(userSourceType);
        }
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



    public enum LavaSourceType
    {
        AutoDetect,
        YouTube,
        Spotify,
        SoundCloud,
    }



}