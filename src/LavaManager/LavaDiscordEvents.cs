#region

using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink;
using LavaSharp.Config;
using LavaSharp.Helpers;

#endregion

namespace LavaSharp.LavaManager;

[EventHandler]
public class LavaDiscordEvents : ApplicationCommandsModule
{
    [Event]
    public static async Task VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
    {
        if (e.User.Id == client.CurrentUser.Id && e.After.Channel is null)
        {
            _ = Task.Run(async () =>
            {
                LavalinkExtension lava = client.GetLavalink();
                LavalinkSession? node = lava?.ConnectedSessions.First().Value;
                LavalinkGuildPlayer? player = node?.GetGuildPlayer(e.Guild);
                if (player is null)
                {
                    return;
                }

                try
                {
                    await CurrentPlayData.CurrentExecutionChannel.SendMessageAsync(
                        "🔊 | I got disconnected from the voice channel. Stopping player and reset queue...");
                }
                catch (Exception)
                {
                    // ignored
                }

                await LavaQueue.DisconnectAndReset(player);
            });
        }
        else if (e.User.Id != client.CurrentUser.Id && e.Before.Channel is not null && e.Before.Channel.Users.Count == 1)
        {
            _ = Task.Run(async () =>
            {
                LavalinkExtension lava = client.GetLavalink();
                LavalinkSession? node = lava?.ConnectedSessions.First().Value;
                LavalinkGuildPlayer? player = node?.GetGuildPlayer(e.Guild);
                if (player is null)
                {
                    return;
                }

                bool DelayActive = bool.Parse(BotConfig.GetConfig("MainConfig", "AutoLeaveOnEmptyChannelDelayActive"));
                if (DelayActive)
                {
                    int delay = int.Parse(BotConfig.GetConfig("MainConfig", "AutoLeaveOnEmptyChannelDelay"));
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                    if (e.Before.Channel.Users.Count > 1)
                    {
                        return;
                    }
                }
                await CurrentPlayData.CurrentExecutionChannel.SendMessageAsync(
                    "🔊 | I got left alone in the voice channel. Stopping player and reset queue...");
                await LavaQueue.DisconnectAndReset(player);
            });
        }
        
    }
}