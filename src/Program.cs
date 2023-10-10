using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using LavaSharp.Config;
using LavaSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Reflection;

namespace LavaSharp;

public class CurrentApplicationData
{
    public static DiscordClient? Client { get; set; }
    public static DiscordUser? BotApplication { get; set; }
}

internal class Program
{
    private static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        var logger = Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
        logger.Information("Starting LavaSharp...");

        string DcApiToken = "";
        try
        {
            DcApiToken = BotConfig.GetConfig()["MainConfig"]["Discord_Token"];
        }
        catch
        {
            logger.Fatal("Discord Token not found in config.ini. Check example_config.ini and add the Token.");
            Console.ReadKey();
            Environment.Exit(41);
        }

        IServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging(lb => lb.AddSerilog())
            .BuildServiceProvider();

        DiscordClient discord = new(new DiscordConfiguration
        {
            Token = DcApiToken,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = LogLevel.Debug,
            Intents = DiscordIntents.All,
            LogTimestampFormat = "MMM dd yyyy - HH:mm:ss tt",
            DeveloperUserId = GlobalProperties.BotOwnerId,
            ServiceProvider = serviceProvider
        });
        discord.RegisterEventHandlers(Assembly.GetExecutingAssembly());
        discord.ClientErrored += Discord_ClientErrored;
        discord.Ready += Discord_Ready;
        discord.UseInteractivity(new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(5)
        });
        var appCommands = discord.UseApplicationCommands(new ApplicationCommandsConfiguration
        {
            ServiceProvider = serviceProvider,
        });
        ulong guildId = ulong.Parse(BotConfig.GetConfig()["MainConfig"]["DiscordServerID"]);
        appCommands.RegisterGuildCommands(Assembly.GetExecutingAssembly(), guildId);
        await discord.ConnectAsync();
        await LavalinkManager.ConnectAsync(discord);
        await StartTasks(discord);
        await Task.Delay(-1);
    }

    private static Task StartTasks(DiscordClient discord)
    {
        UpdatePresence(discord);
        return Task.CompletedTask;
    }

    private static Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
    {
        sender.Logger.LogInformation($"{sender.CurrentUser.UsernameWithDiscriminator} is Online and Ready!");
        CurrentApplicationData.Client = sender;
        CurrentApplicationData.BotApplication = sender.CurrentUser;
        return Task.CompletedTask;
    }

    private static Task Discord_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
    {
        sender.Logger.LogError($"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}");
        sender.Logger.LogError($"Stacktrace: {e.Exception.GetType()}: {e.Exception.StackTrace}");
        return Task.CompletedTask;
    }


    private static async Task UpdatePresence(DiscordClient sender)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        while (true)
        {
            var config = BotConfig.GetConfig()["MainConfig"];
            if (bool.TryParse(config["ShowCurrentSongInPresence"], out var showCurrentSongInPresence) && showCurrentSongInPresence)
            {
                var lava = sender.GetLavalink();
                var node = lava.ConnectedSessions.First().Value;
                var player = node.GetGuildPlayer(await sender.GetGuildAsync(ulong.Parse(config["DiscordServerID"])));

                if (player != null && player.CurrentTrack != null)
                {
                    string trackTitle = player.CurrentTrack.Info.Title;
                    if (trackTitle.Length > 64)
                    {
                        trackTitle = trackTitle.Substring(0, 64);
                    }
                    await sender.UpdateStatusAsync(new DiscordActivity(trackTitle, ActivityType.ListeningTo));
                }
                else
                {
                    await sender.UpdateStatusAsync(new DiscordActivity("Nothing", ActivityType.ListeningTo));
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }



}


public static class GlobalProperties
{
    public static ulong BotOwnerId { get; } = ulong.Parse(BotConfig.GetConfig()["MainConfig"]["BotOwnerId"]);
}