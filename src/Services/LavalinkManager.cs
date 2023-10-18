#region

using DisCatSharp;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using LavaSharp.Config;
using Microsoft.Extensions.Logging;

#endregion

namespace LavaSharp.Services;

public class LavalinkManager
{
    public static LavalinkExtension? LavalinkExtension;
    public static LavalinkSession? LavalinkSession;

    public static LavalinkConfiguration LavaConfig()
    {
        string LHost = BotConfig.GetConfig("Lavalink", "Host");
        int LPort = int.Parse(BotConfig.GetConfig("Lavalink", "Port"));
        string LPass = BotConfig.GetConfig("Lavalink", "Password");
        var endpoint = new ConnectionEndpoint
        {
            Hostname = LHost,
            Port = LPort
        };

        var lavalinkConfig = new LavalinkConfiguration
        {
            Password = LPass,
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };
        return lavalinkConfig;
    }

    public static async Task ConnectAsync(DiscordClient client)
    {
        try
        {
            client.Logger.LogInformation("Connecting to Lavalink...");
            LavalinkExtension = client.UseLavalink();
            LavalinkSession = await LavalinkExtension.ConnectAsync(LavaConfig());
            client.Logger.LogInformation("Connected to Lavalink.");
        }
        catch (Exception ex)
        {
            client.Logger.LogCritical(ex,
                "Lavalink failed to connect. Please Check your Lavalink Config in config.ini. " +
                "Check if Lavalink v4 is running and the correct host/port/password is set.");
            Environment.Exit(40);
        }
    }
}