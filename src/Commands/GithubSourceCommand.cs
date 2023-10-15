using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace LavaSharp.Commands;

public class GithubSourceCommand : ApplicationCommandsModule
{
    [SlashCommand("source", "Shows the source code for the bot.")]
    public static async Task Github(InteractionContext ctx)
    {
        var embed = new DiscordEmbedBuilder();
        var link = "https://github.com/FabiChan99/LavaSharp";
        embed.WithTitle("This project is open source!");
        embed.WithDescription($"You can find the source code for this bot at {link}.\n You can host your own instance of this bot if you want to.");
        embed.WithThumbnail("https://raw.githubusercontent.com/FabiChan99/LavaSharp/master/Assets/BotIcon.png");
        embed.WithColor(DiscordColor.Blurple);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }
}