using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banlink.Handlers;
using Banlink.Utilities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Neo4j.Driver;

namespace Banlink.Commands
{
    public class BotOwnerMisc : BaseCommandModule
    {
        [Command("guildcount")]
        [Hidden]
        [RequireOwner]
        public async Task Guildcount(CommandContext ctx)
        {
            var guilds = ctx.Client.Guilds.Count;
            await ctx.RespondAsync($"The bot is in {guilds} servers!");
        }

        [Command("membercount")]
        [Hidden]
        [RequireOwner]
        public async Task Membercount(CommandContext ctx)
        {
            var memberCount = ctx.Client.Guilds.Values.Sum(server => server.MemberCount);

            await ctx.RespondAsync($"The bot is serving {memberCount} members!");
        }

        [Command("banfrom")]
        [Hidden]
        [RequireOwner]
        public async Task Banfrom(CommandContext ctx, string userId, string serverID, [RemainingText] string reason)
        {
            await ctx.TriggerTypingAsync();

            var bannedMemberId = userId;
            var guildId = serverID;
            if (!ServerUtilities.IsInServer(guildId.ToString())) return; // ignore
            var config = Configuration.ReadConfig(Banlink.ConfigPath);
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            var servers = await driver.GetAllNodesDirectionallyFromGivenNode(guildId.ToString());
            foreach (var server in servers)
                // Realistically this could just be First.
            foreach (var value in server.Values)
            {
                var serverId = value.Value.As<INode>() // Convert to INode
                    .Properties.GetValueOrDefault("id").As<string>(); // Get "id" attribute and convert to string
                var originalBanReason = reason;
                if (string.IsNullOrEmpty(originalBanReason))
                {
                    originalBanReason = "No reason given.";
                }

                if (!GuildBansHandler.AlreadyBannedFrom.Contains($"{serverId}-{bannedMemberId}"))
                {
                    await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
                    {
                        IsTTS = false,
                        Content =
                            $"Banning user `{bannedMemberId}`" +
                            $"from server `{serverId}` - Reason: `{originalBanReason}` " +
                            $"- Manually initiated"
                    });

                    await GuildBansHandler.BanUserIdFromServer(ctx.Client, ulong.Parse(bannedMemberId), serverId,
                        $"Manually initiated ban.\nOriginal ban reason: {originalBanReason}");
                    GuildBansHandler.AlreadyBannedFrom.Add($"{serverId}-{bannedMemberId}");
                }
            }
        }
    }
}