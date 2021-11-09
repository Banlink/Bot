using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banlink.Utilities;
using Colorful;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Neo4j.Driver;

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

        [Command("getnodes")]
        public async Task GetNodes(CommandContext ctx, string rootNodeId)
        {
            var config = Configuration.ReadConfig("config.toml");
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            var nodes = await driver.GetAllNodeDirectionallyFromGivenNode(rootNodeId);
            var message = $"Total nodes: {nodes.Count}\n";
            foreach (IRecord node in nodes)
            {
                foreach (var value in node.Values)
                {
                    message += ($"{value.Value.As<INode>().Properties.GetValueOrDefault("id")}\n");
                }
            }

            await ctx.RespondAsync(message);
        }
    }
}