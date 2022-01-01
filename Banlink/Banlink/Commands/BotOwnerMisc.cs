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

        private static int GuildComparer(KeyValuePair<ulong, DiscordGuild> g1, KeyValuePair<ulong, DiscordGuild> g2)
        {
            if (g1.Value.MemberCount > g2.Value.MemberCount)
            {
                return -1;
            }
            return 1;
        }
        
        [Command("serverlist")]
        [Hidden]
        [RequireOwner]
        public async Task Serverlist(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var list = ctx.Client.Guilds.ToList();
            list.Sort(GuildComparer);
            string message = "";
            int total = 0;
            foreach (var guild in list)
            {
                message += $"{guild.Value.Name} - {guild.Value.Id} : {guild.Value.MemberCount}\n";
                total += guild.Value.MemberCount;
            }

            message += $"\nTotal members: {total}";
            await ctx.RespondAsync(message);
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