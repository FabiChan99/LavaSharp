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
    [SlashCommand("RequireDJRole", "Enable or disable the DJ role requirement.")]
    public async Task RequireDJRole(InteractionContext ctx,
        [Option("setActive", "Set the active state.")]
        bool setActive)
    {
        var cfg = BotConfig.GetConfig("MainConfig", "RequireDJRole");
        var isAlreadyActive = bool.Parse(cfg);
        if (isAlreadyActive == setActive)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The DJ role requirement is already " +
                                                     (setActive ? "enabled" : "disabled") + ".");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }

        BotConfig.SetConfig("MainConfig", "RequireDJRole", setActive);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("🛠️ | The DJ role requirement has been " +
                                                                (setActive ? "``enabled``" : "``disabled``") + ".")
                .AsEphemeral());
    }

    [SlashCommand("SkipAndStopButtons", "Enable or disable the skip and stop buttons in the Now Playing embed.")]
    public async Task SkipAndStopButtons(InteractionContext ctx,
        [Option("setActive", "Set the active state.")]
        bool setActive)
    {
        var cfg = BotConfig.GetConfig("MainConfig", "SkipAndStopButtons");
        var isAlreadyActive = bool.Parse(cfg);
        if (isAlreadyActive == setActive)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The skip and stop buttons are already " +
                                                     (setActive ? "enabled" : "disabled") + ".");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }

        BotConfig.SetConfig("MainConfig", "SkipAndStopButtons", setActive);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("🛠️ | The skip and stop buttons have been " +
                                                                (setActive ? "``enabled``" : "``disabled``") + ".")
                .AsEphemeral());
    }

    [SlashCommand("ShowCurrentSongInPresence", "Enable or disable the current song in presence.")]
    public async Task ShowCurrentSongInPresence(InteractionContext ctx,
        [Option("setActive", "Set the active state.")]
        bool setActive)
    {
        var cfg = BotConfig.GetConfig("MainConfig", "ShowCurrentSongInPresence");
        var isAlreadyActive = bool.Parse(cfg);
        if (isAlreadyActive == setActive)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The current song in presence is already " +
                                                     (setActive ? "enabled" : "disabled") + ".");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }

        BotConfig.SetConfig("MainConfig", "ShowCurrentSongInPresence", setActive);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("🛠️ | The current song in presence has been " +
                                                                (setActive ? "``enabled``" : "``disabled``") + ".")
                .AsEphemeral());
    }
    [SlashCommand("AutoDisconnect", "Enable or disable the auto-disconnect when all users left the channel.")]
    public async Task AutoDisconnect(InteractionContext ctx,
        [Option("setActive", "Set the active state.")]
        bool setActive)
    {
        var cfg = BotConfig.GetConfig("MainConfig", "AutoLeaveOnEmptyChannel");
        var isAlreadyActive = bool.Parse(cfg);
        if (isAlreadyActive == setActive)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The auto-disconnect is already " +
                                                     (setActive ? "enabled" : "disabled") + ".");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }

        BotConfig.SetConfig("MainConfig", "AutoLeaveOnEmptyChannel", setActive);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("🛠️ | The auto-disconnect has been " +
                                                                (setActive ? "``enabled``" : "``disabled``") + ".")
                .AsEphemeral());
    }
    
    [SlashCommand("AutoDisconnectDelay", "Set the delay for the auto-disconnect when all users left the channel.")]
    public async Task AutoDisconnectDelay(InteractionContext ctx,
        [Option("delay", "Set the delay in seconds.")]
        int delay)
    {
        if (delay < 1 || delay > 300)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The delay must be between ``1`` and ``300`` seconds.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }
        var cfg = BotConfig.GetConfig("MainConfig", "AutoLeaveOnEmptyChannelDelay");
        var isAlreadyActive = int.Parse(cfg);
        if (isAlreadyActive == delay)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The auto-disconnect delay is already set to " + delay + " seconds.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }

        BotConfig.SetConfig("MainConfig", "AutoLeaveOnEmptyChannelDelay", delay.ToString());
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("🛠️ | The auto-disconnect delay has been set to " + delay + " seconds.")
                .AsEphemeral());
    }
    
    [SlashCommand("AutoDisconnectDelayActive", "Enable or disable the delay for the auto-disconnect when all users left the channel.")]
    public async Task AutoDisconnectDelayActive(InteractionContext ctx,
        [Option("setActive", "Set the active state.")]
        bool setActive)
    {
        var cfg = BotConfig.GetConfig("MainConfig", "AutoLeaveOnEmptyChannelDelayActive");
        var isAlreadyActive = bool.Parse(cfg);
        if (isAlreadyActive == setActive)
        {
            var embed = EmbedGenerator.GetErrorEmbed("The auto-disconnect delay is already " +
                                                     (setActive ? "enabled" : "disabled") + ".");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
            return;
        }

        BotConfig.SetConfig("MainConfig", "AutoLeaveOnEmptyChannelDelayActive", setActive);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("🛠️ | The auto-disconnect delay has been " +
                                                                (setActive ? "``enabled``" : "``disabled``") + ".")
                .AsEphemeral());
    }
    
    
}