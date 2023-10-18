#region

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using LavaSharp.Config;
using LavaSharp.Helpers;

#endregion

namespace LavaSharp.Attributes
{
    public sealed class ApplicationRequireExecutorInVoice : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            if (ctx.Member.VoiceState?.Channel is null)
            {
                var embed = EmbedGenerator.GetErrorEmbed("You must be in a voice channel to use this command.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return false;
            }

            return true;
        }
    }

    public sealed class RequireRunningPlayer : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedSessions.First().Value;
            var player = node.GetGuildPlayer(ctx.Guild);
            if (player is null)
            {
                var errorembed = EmbedGenerator.GetErrorEmbed("I'm not connected to a voice channel.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return false;
            }

            return true;
        }
    }

    public sealed class EnsureGuild : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            if (ctx.Guild is null)
            {
                var errorembed = EmbedGenerator.GetErrorEmbed("This command can only be used in a server.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return false;
            }

            return true;
        }
    }

    public sealed class EnsureMatchGuildId : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            var configuredId = ulong.Parse(BotConfig.GetConfig("MainConfig", "DiscordServerID"));
            if (ctx.Guild.Id != configuredId)
            {
                var errorembed =
                    EmbedGenerator.GetErrorEmbed("This command can only be used in the configured server.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return false;
            }

            return true;
        }
    }
    
    public sealed class CheckDJ : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {

            IReadOnlyCollection<DiscordMember> voicemembers = ctx.Member.VoiceState.Channel.Users;
            bool isPlaying = false;
            try
            {
                var ct = ctx.Client.GetLavalink().ConnectedSessions!.First().Value.GetGuildPlayer(ctx.Guild).CurrentTrack;
                if (ct != null)
                {
                    isPlaying = true;
                }
            }
            catch (Exception )
            {
                isPlaying = false;
            }

            if (!isPlaying)
            {
                if (voicemembers.Count == 1)
                {
                    return true;
                }
            }
            else if (isPlaying)
            {
                if (voicemembers.Count == 2)
                {
                    if (voicemembers.Any(x => x.Id == ctx.Client.CurrentUser.Id) && voicemembers.Any(x => x.Id == ctx.Member.Id))
                    {
                        return true;
                    }
                }
            }
            
            bool djconf = bool.Parse(BotConfig.GetConfig("MainConfig", "RequireDJRole"));
            if (!djconf)
            {
                return true;
            }
            bool isDJ = false;
            bool isCtxAdminorManager = ctx.Member.Permissions.HasPermission(Permissions.Administrator) ||
                                      ctx.Member.Permissions.HasPermission(Permissions.ManageGuild);
            if (isCtxAdminorManager)
            {
                isDJ = true;
            }
            var checkRoleifuserhasdj = ctx.Member.Roles.Any(x => x.Name == "DJ");
            if (checkRoleifuserhasdj)
            {
                isDJ = true;
            }
            if (!isDJ)
            {
                DiscordEmbedBuilder errorembed = EmbedGenerator.GetErrorEmbed("You must have a DJ role called ``DJ`` to use this command.");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return false;
            }
            return isDJ;

        }
    }
}