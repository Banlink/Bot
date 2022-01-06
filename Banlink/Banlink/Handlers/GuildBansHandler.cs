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
        private static readonly List<string> AlreadyUnbannedFrom = new List<string>();
        public static readonly List<string> AlreadyBannedFrom = new List<string>();

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
                var serverId = value.Value.As<INode>() // Convert to INode
                    .Properties.GetValueOrDefault("id").As<string>(); // Get "id" attribute and convert to string
                var ban = await args.Guild.GetBanAsync(args.Member);
                var originalBanReason = ban.Reason;
                if (string.IsNullOrEmpty(originalBanReason))
                {
                    originalBanReason = "No reason given.";
                }

                if (!AlreadyBannedFrom.Contains($"{serverId}-{bannedMemberId}"))
                {
                    var guild = await client.GetGuildAsync(ulong.Parse(serverId));
                    await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
                    {
                        IsTTS = false,
                        Content =
                            $"Banning user `{bannedMemberId}` - `{args.Member.Username}#{args.Member.Discriminator}` " +
                            $"from server `{serverId}` - `{guild.Name}` - Reason: `{originalBanReason}` " +
                            $"- Ban origin server: `{args.Guild.Name}` - `{guildId}`"
                    });

                    await BanUserIdFromServer(client, bannedMemberId, serverId,
                        "Banned due to Banlink link with server. " +
                        $"\nServer Name: {args.Guild.Name} - ID: {guildId}" +
                        $"\nOriginal ban reason: {originalBanReason}",
                        guild);
                    AlreadyBannedFrom.Add($"{serverId}-{bannedMemberId}");
                }
            }
        }

        public static async Task BanUserIdFromServer(
            DiscordClient client,
            ulong userId,
            string serverId,
            string reason,
            DiscordGuild server)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                {
                    reason = "No reason! This is a bug! Please tell Whanos#0621!";
                }
                // 512 is max length for ban reasons.
                if (reason.Length > 512) {
                    reason = reason.Substring(0, 512);
                }
                await server.BanMemberAsync(userId, 0, reason);
            }
            catch (UnauthorizedException e)
            {
                Console.WriteLine($"[UnauthorizedException] Could not ban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[UnauthorizedException] Could not ban user {userId} from server {serverId}!\n```{e}```"
                });
            }
            catch (NotFoundException e)
            {
                Console.WriteLine($"[NotFoundException] Could not ban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[NotFoundException] Could not ban user {userId} from server {serverId}!\n```{e}```"
                });
            }
            catch (BadRequestException e)
            {
                Console.WriteLine($"[BadRequestException] Could not ban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[BadRequestException] Could not ban user {userId} from server {serverId}!\n```{e}```"
                });
            }
            catch (ServerErrorException e)
            {
                Console.WriteLine($"[ServerErrorException] Could not ban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[ServerErrorException] Could not ban user {userId} from server {serverId}!\n```{e}```"
                });
            }
        }

        private static async Task UnbanUserIdFromServer(
            DiscordClient client,
            ulong userId,
            string serverId,
            string reason,
            DiscordGuild server)
        {
            try
            {
                await server.UnbanMemberAsync(userId, reason);
            }
            catch (UnauthorizedException e)
            {
                Console.WriteLine($"[UnauthorizedException] Could not unban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[UnauthorizedException] Could not unban user {userId} from server {serverId}!\n```{e}```"
                });
            }
            catch (NotFoundException e)
            {
                Console.WriteLine($"[NotFoundException] Could not unban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[NotFoundException] Could not unban user {userId} from server {serverId}!\n```{e}```"
                });
            }
            catch (BadRequestException e)
            {
                Console.WriteLine($"[BadRequestException] Could not unban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[BadRequestException] Could not unban user {userId} from server {serverId}!\n```{e}```"
                });
            }
            catch (ServerErrorException e)
            {
                Console.WriteLine($"[ServerErrorException] Could not unban user {userId} from server {serverId}!");
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder()
                {
                    Content = $"[ServerErrorException] Could not unban user {userId} from server {serverId}!\n```{e}```"
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
                    if (!AlreadyUnbannedFrom.Contains($"{serverId}-{unbannedMemberId}"))
                    {
                        var guild = await client.GetGuildAsync(ulong.Parse(serverId));
                        await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
                        {
                            IsTTS = false,
                            Content = $"Unbanning user `{unbannedMemberId}` - `{args.Member.Username}#{args.Member.Discriminator}` " +
                                      $"from server `{serverId}` - `{guild.Name}` - Unban origin server: `{args.Guild.Name}` - `{guildId}`"
                        });
                        await UnbanUserIdFromServer(client, unbannedMemberId, serverId,
                            "Unbanned due to Banlink link with server. " +
                            $"\nServer name: {args.Guild.Name} - ID: {guildId}",
                            guild);
                        AlreadyUnbannedFrom.Add($"{serverId}-{unbannedMemberId}");
                    }
                }
            }
        }
    }
}