using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using DisCatSharp.Exceptions;
using LavaSharp.Attributes;
using LavaSharp.Enums;
using LavaSharp.Helpers;
using LavaSharp.LavaManager;
using Microsoft.VisualBasic;

namespace LavaSharp.Commands;



public class PlayCommand : ApplicationCommandsModule
{
    /*

    INTIAL CODE

    [SlashCommand("play", "Plays a song.")]
    public async Task Play(InteractionContext ctx, [Option("query", "The query to search for (URL or Song Name")] string query)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member.VoiceState?.Channel;
        if (channel is null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You must be in a voice channel!"));
            return;
        }
        LavalinkGuildPlayer? lavaPlayer = null;
        if (player != null && player.Channel != channel)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You must be in the same voice channel!"));
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Failed to connect to voice channel!"));
            return;
        }
        SourceType sourceType = LinkCheck.GetSourceType(query);
        // prefix query with source type if it is a link
        var searchtype = LavalinkSearchType.Youtube;
        if (LinkCheck.isLink(query))
        {
            switch (sourceType)
            {
                case SourceType.YouTube:
                    query = "ytsearch:" + query;
                    break;
                case SourceType.Spotify:
                    query = "spsearch:" + query;

                    break;
                case SourceType.SoundCloud:
                    query = "scsearch:" + query;
                    break;
                default:
                    query = "ytsearch:" + query;
                    break;
            }
        }
        string search = query;
        if (search.StartsWith("ytsearch:"))
        {
            search = search.Replace("ytsearch:", "");
            searchtype = LavalinkSearchType.Youtube;
        }
        else if (search.StartsWith("scsearch:"))
        {
            search = search.Replace("scsearch:", "");
            searchtype = LavalinkSearchType.SoundCloud;
        }
        else if (search.StartsWith("spsearch:"))
        {
            search = search.Replace("spsearch:", "");
            searchtype = LavalinkSearchType.Spotify;
        }

        var loadResult = await lavaPlayer.LoadTracksAsync(searchtype, search);
        LavalinkTrack track = loadResult.LoadType switch
        {
            LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
            LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
            LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
            LavalinkLoadResultType.Error => throw new InvalidOperationException($"Error loading track: {loadResult}"),
            LavalinkLoadResultType.Empty => throw new InvalidOperationException("No results found."),
            _ => throw new InvalidOperationException("Unexpected load result type.")
        };
        bool isPlaying = lavaPlayer.CurrentTrack is not null;
        //lavaPlayer.AddToQueue(new QueueEntry(), track);
        if (isPlaying)
        {
            LavaQueue.queue.Enqueue(track);
            int queueSize = LavaQueue.queue.Count;
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Added {track.Info.Title} to the queue!"));
            return;
        }
        else
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Now playing {track.Info.Title}")); ;
            lavaPlayer.TrackEnded += (sender, e) => LavaQueue.PlaybackFinished(sender, e, ctx);
            await lavaPlayer.PlayAsync(track);
            return;
        }
    }
    */

    /*

    [SlashCommand("play", "Plays a song.")]
    public async Task Play(InteractionContext ctx,
        [Option("query", "The query to search for (URL or Song Name")] string query,
        [Option("SourceType", "[Optional] The Source where to query from(Youtube, Spotify, SoundCloud")] LavaSourceType searchType = LavaSourceType.YouTube)
    {
        LavalinkSearchType lavalinkSearchType = SearchType(searchType);
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member.VoiceState?.Channel;
        if (channel is null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You must be in a voice channel!"));
            return;
        }
        LavalinkGuildPlayer? lavaPlayer = null;
        if (player != null && player.Channel != channel)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You must be in the same voice channel!"));
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Failed to connect to voice channel!"));
            return;
        }
        Console.WriteLine(query);
        var loadResult = await lavaPlayer.LoadTracksAsync(lavalinkSearchType, query);
        LavalinkTrack track = loadResult.LoadType switch
        {
            LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
            LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
            LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
            LavalinkLoadResultType.Error => throw new InvalidOperationException($"Error loading track: {loadResult}"),
            LavalinkLoadResultType.Empty => throw new InvalidOperationException("No results found."),
            _ => throw new InvalidOperationException("Unexpected load result type.")
        };
        bool isPlaying = lavaPlayer.CurrentTrack is not null;
        //lavaPlayer.AddToQueue(new QueueEntry(), track);
        if (isPlaying)
        {
            LavaQueue.queue.Enqueue(track);
            int queueSize = LavaQueue.queue.Count;
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Added {track.Info.Title} to the queue!"));
            return;
        }
        else
        {
            Console.WriteLine(track.Info.Uri);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Now playing {track.Info.Title}")); ;
            lavaPlayer.TrackEnded += (sender, e) => LavaQueue.PlaybackFinished(sender, e, ctx);
            await lavaPlayer.PlayAsync(track);
            return;
        }
    }
    */

    [ApplicationRequireExecutorInVoice]
    [SlashCommand("play", "Plays a song.")]
    public static async Task Play(InteractionContext ctx, [Option("query", "The query to search for (URL or Song Name)")] string query)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedSessions.First().Value;
        var player = node.GetGuildPlayer(ctx.Guild);
        var channel = ctx.Member.VoiceState?.Channel;
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

        var loadResult = await lavaPlayer.LoadTracksAsync(LavalinkSearchType.Youtube, query);
        LavalinkTrack track = loadResult.LoadType switch
        {
            LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
            LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
            LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
            LavalinkLoadResultType.Error => throw new InvalidOperationException($"Error loading track: {loadResult}"),
            LavalinkLoadResultType.Empty => throw new FileNotFoundException("No Results"),
            _ => throw new InvalidOperationException("Unexpected load result type.")
        };
        bool isPlaying = lavaPlayer.CurrentTrack is not null;

        if (isPlaying)
        {
            LavaQueue.queue.Enqueue(track);


            var playEmbed = EmbedGenerator.GetPlayEmbed(track);
            playEmbed.WithDescription($"Added {SongResolver.GetTrackInfo(track)} to the queue!");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(playEmbed));
            return;
        }
        else
        {
            var playEmbed = EmbedGenerator.GetPlayEmbed(track);
            playEmbed.WithDescription($"Now playing {SongResolver.GetTrackInfo(track)}");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(playEmbed));

            lavaPlayer.TrackEnded += (sender, e) => LavaQueue.PlaybackFinished(sender, e, ctx);
            await lavaPlayer.PlayAsync(track);
            return;
        }
    }




    private LavalinkSearchType SearchType(LavaSourceType sourceType)
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
        YouTube,
        Spotify,
        SoundCloud
    }



}