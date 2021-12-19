using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Banlink.Utilities;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Neo4j.Driver;

namespace Banlink.Handlers
{
    public static class GuildBansHandler
    {
        public static async Task BanHandler(DiscordClient client, GuildBanAddEventArgs args)
        {
            var bannedMemberId = args.Member.Id;
            var guildId = args.Guild.Id;
            if (!ServerUtilities.IsInServer(guildId.ToString())) return; // ignore
            var config = Configuration.ReadConfig(Banlink.ConfigPath);
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            var servers = await driver.GetAllNodesDirectionallyFromGivenNode(guildId.ToString());
            foreach (var server in servers)
                // Realistically this could just be First.
            foreach (var value in server.Values)
            {
                var serverId = value.Value.As<INode>().Properties.GetValueOrDefault("id").As<string>();
                await BanUserIdFromServer(client, bannedMemberId, serverId,
                    "Banned due to Banlink link with server. " +
                    $"\nServer name: {args.Guild.Name} - ID: {guildId}");
            }
        }

        private static async Task BanUserIdFromServer(
            DiscordClient client,
            ulong userId,
            string serverId,
            string reason = null)
        {
            try
            {
                var server = await client.GetGuildAsync(ulong.Parse(serverId));
                await server.BanMemberAsync(userId, 0, reason);
            }
            catch (UnauthorizedException e)
            {
                Console.WriteLine($"[UnauthorizedException] Could not ban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[UnauthorizedException] Could not ban user {userId} from server {serverId}!"
                });
            }
            catch (NotFoundException e)
            {
                Console.WriteLine($"[NotFoundException] Could not ban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[NotFoundException] Could not ban user {userId} from server {serverId}!"
                });
            }
            catch (BadRequestException e)
            {
                Console.WriteLine($"[BadRequestException] Could not ban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[BadRequestException] Could not ban user {userId} from server {serverId}!"
                });
            }
            catch (ServerErrorException e)
            {
                Console.WriteLine($"[ServerErrorException] Could not ban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[ServerErrorException] Could not ban user {userId} from server {serverId}!"
                });
            }
        }

        private static async Task UnbanUserIdFromServer(
            DiscordClient client,
            ulong userId,
            string serverId,
            string reason = null)
        {
            try
            {
                var server = await client.GetGuildAsync(ulong.Parse(serverId));
                await server.UnbanMemberAsync(userId, reason);
            }
            catch (UnauthorizedException e)
            {
                Console.WriteLine($"[UnauthorizedException] Could not unban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[UnauthorizedException] Could not unban user {userId} from server {serverId}!"
                });
            }
            catch (NotFoundException e)
            {
                Console.WriteLine($"[NotFoundException] Could not unban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[NotFoundException] Could not unban user {userId} from server {serverId}!"
                });
            }
            catch (BadRequestException e)
            {
                Console.WriteLine($"[BadRequestException] Could not unban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[BadRequestException] Could not unban user {userId} from server {serverId}!"
                });
            }
            catch (ServerErrorException e)
            {
                Console.WriteLine($"[ServerErrorException] Could not unban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[ServerErrorException] Could not unban user {userId} from server {serverId}!"
                });
            }
        }

        public static async Task UnbanHandler(DiscordClient client, GuildBanRemoveEventArgs args)
        {
            var unbannedMemberId = args.Member.Id;
            var guildId = args.Guild.Id;
            if (!ServerUtilities.IsInServer(guildId.ToString())) return; // ignore
            var config = Configuration.ReadConfig(Banlink.ConfigPath);
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            var servers = await driver.GetAllNodesDirectionallyFromGivenNode(guildId.ToString());
            foreach (var server in servers)
            {
                foreach (var value in server.Values)
                {
                    var serverId = value.Value.As<INode>().Properties.GetValueOrDefault("id").As<string>();
                    Console.WriteLine(serverId);
                    await UnbanUserIdFromServer(client, unbannedMemberId, serverId,
                        "Unbanned due to Banlink link with server. " +
                        $"\nServer name: {args.Guild.Name} - ID: {guildId}");
                }
            }
        }
    }
}