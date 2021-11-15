using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Banlink.Utilities;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Neo4j.Driver;

namespace Banlink.Handlers
{
    public static class GuildBansHandler
    {
        public static async Task BanHandler(DiscordClient client, GuildBanAddEventArgs args)
        { 
            var bannedMemberId = args.Member.Id;
            var guildId = args.Guild.Id;
            if (!ServerUtilities.IsInServer(guildId.ToString()))
            {
                return; // ignore
            }
            var config = Configuration.ReadConfig(Banlink.ConfigPath);
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            var servers = await driver.GetAllNodesDirectionallyFromGivenNode(guildId.ToString());
            foreach (IRecord server in servers)
            {
                // Realistically this could just be First.
                foreach (var value in server.Values)
                {
                    var serverId = value.Value.As<INode>().Properties.GetValueOrDefault("id").As<string>();
                    await BanUserIdFromServer(client, bannedMemberId, serverId, 
                        $"Banned due to Banlink link with server. Server name {args.Guild.Name} - ID: {guildId}");
                }
            }
        }

        private static async Task BanUserIdFromServer(
            DiscordClient client, 
            ulong userId, 
            string serverId,
            string reason = null)
        {
            var server = await client.GetGuildAsync(ulong.Parse(serverId));
            await server.BanMemberAsync(userId, 0, reason);
        }

        private static async Task UnbanUserIdFromServer(
            DiscordClient client,
            ulong userId,
            string serverId,
            string reason = null)
        {
            var server = await client.GetGuildAsync(ulong.Parse(serverId));
            await server.UnbanMemberAsync(userId, reason);
        }

        public static async Task UnbanHandler(DiscordClient client, GuildBanRemoveEventArgs args)
        {
            var unbannedMemberId = args.Member.Id;
            var guildId = args.Guild.Id;
            if (!ServerUtilities.IsInServer(guildId.ToString()))
            {
                return; // ignore
            }
            var config = Configuration.ReadConfig(Banlink.ConfigPath);
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            var servers = await driver.GetAllNodesDirectionallyFromGivenNode(guildId.ToString());
            foreach (IRecord server in servers)
            {
                // Realistically this could just be First.
                foreach (var value in server.Values)
                {
                    var serverId = value.Value.As<INode>().Properties.GetValueOrDefault("id").As<string>();
                    Console.WriteLine(serverId);
                    await UnbanUserIdFromServer(client, unbannedMemberId, serverId, 
                        $"Unbanned due to Banlink link with server. Server name {args.Guild.Name} - ID: {guildId}");
                }
            }
        }
    }
}