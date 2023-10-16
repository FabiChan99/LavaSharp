using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using LavaSharp.Config;
using LavaSharp.Helpers;

namespace LavaSharp.Commands;

[ApplicationCommandRequireUserPermissions(Permissions.ManageGuild)]
[SlashCommandGroup("settings", "Change the bot's settings.")]
public class SettingsCommand : ApplicationCommandsModule
{
    [ApplicationCommandRequireUserPermissions(Permissions.ManageGuild)]
    [SlashCommand("RequireDJRole", "Enable or disable the DJ role requirement.")]
    public async Task RequireDJRole(InteractionContext ctx, [Option("setActive", "Set the active state.")] bool setActive)
    {
        var cfg = BotConfig.GetConfig();
        var isAlreadyActive = bool.Parse(cfg["MainConfig"]["RequireDJRole"]);
        if (isAlreadyActive == setActive)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The DJ role requirement is already " + (setActive ? "enabled" : "disabled") + ".");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }
        BotConfig.SetConfig("MainConfig", "RequireDJRole", setActive.ToString());
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("🛠️ | The DJ role requirement has been " + (setActive ? "``enabled``" : "``disabled``") + "."));
    }
    [SlashCommand("SkipAndStopButtons", "Enable or disable the skip and stop buttons in the Now Playing embed.")]
    public async Task SkipAndStopButtons(InteractionContext ctx, [Option("setActive", "Set the active state.")] bool setActive)
    {
        var cfg = BotConfig.GetConfig();
        var isAlreadyActive = bool.Parse(cfg["MainConfig"]["SkipAndStopButtons"]);
        if (isAlreadyActive == setActive)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The skip and stop buttons are already " + (setActive ? "enabled" : "disabled") + ".");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }
        BotConfig.SetConfig("MainConfig", "SkipAndStopButtons", setActive.ToString());
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("??️ | The skip and stop buttons have been " + (setActive ? "``enabled``" : "``disabled``") + "."));
    }
    
    [SlashCommand("ShowCurrentSongInPresence", "Enable or disable the current song in presence.")]
    public async Task ShowCurrentSongInPresence(InteractionContext ctx, [Option("setActive", "Set the active state.")] bool setActive)
    {
        var cfg = BotConfig.GetConfig();
        var isAlreadyActive = bool.Parse(cfg["MainConfig"]["ShowCurrentSongInPresence"]);
        if (isAlreadyActive == setActive)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The current song in presence is already " + (setActive ? "enabled" : "disabled") + ".");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }
        BotConfig.SetConfig("MainConfig", "ShowCurrentSongInPresence", setActive.ToString());
        // without embed
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("??️ | The current song in presence has been " + (setActive ? "``enabled``" : "``disabled``") + "."));
    }
    
}