using System.Threading.Tasks;
using Banlink.Utilities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Banlink.Commands
{
    public class Test : BaseCommandModule
    {
        [Command("createlink")]
        public async Task CreateLink(CommandContext ctx, string serverId1, string serverId2)
        {
            var config = Configuration.ReadConfig("config.toml");
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            await driver.CreateServerLink(serverId1, serverId2);
            await ctx.RespondAsync($"{serverId1}:Server-[:LINKED_TO]->{serverId2}:Server");
        }

        [Command("deletenode")]
        public async Task DeleteNode(CommandContext ctx, string serverId)
        {
            var config = Configuration.ReadConfig("config.toml");
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            await driver.DeleteNodeAndRelationshipsToNode(serverId);
            await ctx.RespondAsync($"Detached and deleted node {serverId}");
        }
    }
}