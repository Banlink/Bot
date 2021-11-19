using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banlink.Utilities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Neo4j.Driver;

namespace Banlink.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("createlink")]
        [Hidden]
        [RequireOwner]
        public async Task CreateLink(CommandContext ctx, string serverId1, string serverId2)
        {
            var config = Configuration.ReadConfig("config.toml");
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            await driver.CreateServerLink(serverId1, serverId2);
            await ctx.RespondAsync($"{serverId1}:Server-[:LINKED_TO]->{serverId2}:Server");
        }

        [Command("deletenode")]
        [Hidden]
        [RequireOwner]
        public async Task DeleteNode(CommandContext ctx, string serverId)
        {
            var config = Configuration.ReadConfig("config.toml");
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            await driver.DeleteNodeAndRelationshipsToNode(serverId);
            await ctx.RespondAsync($"Detached and deleted node {serverId}");
        }

        [Command("getnodes")]
        [Hidden]
        [RequireOwner]
        public async Task GetNodes(CommandContext ctx, string rootNodeId)
        {
            var config = Configuration.ReadConfig("config.toml");
            var driver = new Neo4J(config.DbUri, config.Username, config.Password);
            var nodes = await driver.GetAllNodesDirectionallyFromGivenNode(rootNodeId);
            var message = $"Total nodes: {nodes.Count}\n";
            message = nodes.SelectMany(node => node.Values)
                .Aggregate(message, (current, value) =>
                    current + $"{value.Value.As<INode>().Properties.GetValueOrDefault("id")}\n");

            await ctx.RespondAsync(message);
        }
    }
}