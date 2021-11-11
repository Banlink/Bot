using System.Threading.Tasks;
using Banlink.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Banlink.Commands
{
    public class ServerLinking : BaseCommandModule
    {
        [Command("link")]
        [RequirePermissions(Permissions.ManageGuild)]
        [Description("Enter a valid link code to automatically link a server's bans to the current server." +
                     "The can only be used once before it gets automatically deleted. Generate a new one to link more servers.")]
        public async Task Link(CommandContext ctx, string linkCode)
        {
            // TODO: Check if the link code is valid, if it is, link the bans, if not, tell the user it's invalid.
            // TODO: Additionally, check if the link code was already used, if it was, tell the user it's invalid.
            // TODO: You should be able to get server id's from the link code, and then link the bans.
        }

        [Command("forcelink")]
        [RequireOwner]
        [Description("Forcefully link two servers together.")]
        [Hidden]
        public async Task ForceLink(CommandContext ctx, string server1, string server2)
        {
            if (server1 == server2)
            {
                await ctx.RespondAsync("You can't link a server to itself.");
                return;
            }
            if (server1 .Length != 18 || server2.Length != 18)
            {
                await ctx.RespondAsync("Server IDs must be 18 characters long.");
                return;
            }

            if (!ServerUtilities.IsInServer(server1) || !ServerUtilities.IsInServer(server2))
            {
                await ctx.RespondAsync("One of the given servers does not have the bot added!");
                return;
            }
            var config = Configuration.ReadConfig("config.toml");
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            await driver.CreateServerLink(server1, server2);
            await ctx.RespondAsync($"Created link {server1}-[:LINKED_TO]->{server2}");
        }

        [Command("unlink")]
        [RequirePermissions(Permissions.ManageGuild)]
        [Description("Unlink the server this command was ran in from a given server ID. " +
                     "You must be previously linked for this to work.")]
        public async Task Unlink(CommandContext ctx, string serverId)
        {
            if (serverId.Length != 18)
            {
                await ctx.RespondAsync("Server IDs must be 18 characters long.");
                return;
            }
            if (!ServerUtilities.IsInServer(serverId))
            {
                await ctx.RespondAsync("The given server does not have the bot added!");
                return;
            }
            var config = Configuration.ReadConfig("config.toml");
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            await driver.UnlinkServer(serverId, ctx.Guild.Id.ToString());
            await ctx.RespondAsync("Successfully unlinked!");
        }
    }
}