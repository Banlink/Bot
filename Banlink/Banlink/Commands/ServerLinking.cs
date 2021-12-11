using System.Threading.Tasks;
using Banlink.Utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Banlink.Commands
{
    public class ServerLinking : BaseCommandModule
    {
        private static Neo4J _driver;

        public static void GetDriver()
        {
            var config = Configuration.ReadConfig(Banlink.ConfigPath);
            _driver = new Neo4J(config.DbUri, config.Username, config.Password);
        }

        [Command("link")]
        [RequirePermissions(Permissions.ManageGuild)]
        [Description("Enter a valid link code to automatically link a server's bans to the current server. " +
                     "Generate a code with the 'generate' command. This overwrites your previous code, if you had one.")]
        [Cooldown(1, 10, CooldownBucketType.Guild)]
        public async Task Link(CommandContext ctx, string linkCode)
        {
            // TODO: Check if the link code is valid, if it is, link the bans, if not, tell the user it's invalid.
            // TODO: Additionally, check if the link code was already used, if it was, tell the user it's invalid.
            // TODO: You should be able to get server id's from the link code, and then link the bans.
            await ctx.TriggerTypingAsync();
            if (linkCode.Length != 27)
            {
                await ctx.RespondAsync("Invalid link code.");
                return;
            }

            await _driver.ValidateLinkCodeAndLinkServer(linkCode, ctx.Guild.Id.ToString());
            await ctx.RespondAsync($"Successfully linked server!");
        }

        [Command("generate")]
        [RequirePermissions(Permissions.ManageGuild)]
        [Description(
            "Generates a link code for the current server which you can use with the 'link' command in another server.")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task GenerateLinkCode(CommandContext ctx)
        {
            /*
             * Generate link code for current server which can be used to link servers together
             */
            await ctx.TriggerTypingAsync();
            var linkCode = ServerUtilities.GenerateLinkCode(ctx.Guild.Id.ToString());
            await _driver.AssignLinkCodeToServerNode(ctx.Guild.Id.ToString(), linkCode);
            await ctx.RespondAsync($"Here is your link code: `{linkCode}`");
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

            if (server1.Length != 18 || server2.Length != 18)
            {
                await ctx.RespondAsync("Server IDs must be 18 characters long.");
                return;
            }

            if (!ServerUtilities.IsInServer(server1) || !ServerUtilities.IsInServer(server2))
            {
                await ctx.RespondAsync("One of the given servers does not have the bot added!");
                return;
            }

            await _driver.CreateServerLink(server1, server2);
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

            await _driver.UnlinkServer(serverId, ctx.Guild.Id.ToString());
            await ctx.RespondAsync("Successfully unlinked!");
        }
    }
}