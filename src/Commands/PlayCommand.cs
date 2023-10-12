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
    public static async Task Play(InteractionContext ctx, [Option("query", "The query to search for (URL or Song Name)")] string query)
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

        var loadResult = await lavaPlayer.LoadTracksAsync(LavalinkSearchType.Youtube, query);
        LavalinkTrack track;
        try
        {
            track = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                LavalinkLoadResultType.Error => throw new InvalidOperationException(
                    $"Error loading track: {loadResult}"),
                LavalinkLoadResultType.Empty => throw new FileNotFoundException("No Results"),
                _ => throw new InvalidOperationException("Unexpected load result type.")
            };
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
        if (isPlaying)
        {
            LavaQueue.queue.Enqueue((track, ctx.User));
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"🎵 | Added **{track.Info.Title}** to the queue."));
            return;
        }
        else
        {
            CurrentPlayData.track = track;
            CurrentPlayData.user = ctx.User;
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"🎵 | Added **{track.Info.Title}** to the queue."));
            await ctx.Channel.SendMessageAsync(embed: EmbedGenerator.GetCurrentTrackEmbed(track, lavaPlayer));
            RegisterPlaybackFinishedEvent(lavaPlayer, ctx);
            await lavaPlayer.PlayAsync(track);
            return;
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