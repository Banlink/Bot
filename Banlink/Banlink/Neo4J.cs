using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banlink.Utilities;
using DSharpPlus.Entities;
using Neo4j.Driver;

namespace Banlink
{
    public sealed class Neo4J : IDisposable
    {
        private readonly IDriver _driver;
        private bool _disposed;

        public Neo4J(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        private static async Task ReportError(string query, string ex)
        {
            await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
            {
                Content = $"[Neo4JException] ![FATAL]! {query} - {ex}"
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Neo4J()
        {
            Dispose(false);
        }

        public async Task ValidateLinkCodeAndLinkServer(string linkCode, string callingServerId)
        {
            var searchQuery = @"
MATCH (s:Server)
WHERE s.linkCode = $linkCode
RETURN s.id";
            var session = _driver.AsyncSession();
            if (!ServerUtilities.IsInServer(callingServerId)) return;

            try
            {
                var id = await session.WriteTransactionAsync(async tx =>
                {
                    var id = await tx.RunAsync(searchQuery, new {linkCode});
                    return await id.ToListAsync();
                });
                var serverId = id.First().Values.Values.First().As<string>();
                if (serverId == callingServerId) return;
                await CreateServerLink(serverId, callingServerId);
            }
            catch (Neo4jException ex)
            {
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
                {
                    Content = $"[Neo4JException] ![FATAL]! {searchQuery} - {ex}"
                });
                Console.WriteLine($"{searchQuery} - {ex}");
            }
        }

        public async Task AssignLinkCodeToServerNode(string serverId, string linkCode)
        {
            var session = _driver.AsyncSession();
            if (!ServerUtilities.IsInServer(serverId)) return;
            {
                try
                {
                    await session.WriteTransactionAsync(tx =>
                        tx.RunAsync("MATCH (n:Server {id: $id}) SET n.linkCode = $linkCode",
                            new Dictionary<string, object>
                            {
                                {"id", serverId},
                                {"linkCode", linkCode}
                            }));
                }
                catch (Neo4jException ex)
                {
                    await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
                    {
                        Content = $"[Neo4JException] Error assigning link code! - {ex}\n{serverId} - {linkCode}"
                    });
                }
            }
        }

        public async Task DeleteNodeAndRelationshipsToNode(string serverId)
        {
            var query = @"
MATCH (s:Server)
WHERE s.id = $id
DETACH DELETE s";

            var session = _driver.AsyncSession();
            try
            {
                await session.WriteTransactionAsync(async tx => { await tx.RunAsync(query, new {id = serverId}); });
            }
            catch (Neo4jException ex)
            {
                await ReportError(query, ex.ToString());
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task UnlinkServer(string serverId1, string serverid2)
        {
            const string query = @"MATCH (:Server {id: $serverid1})-[r:LINKED_TO]->(:Server {id: $serverid2})
            DELETE r";

            var session = _driver.AsyncSession();
            try
            {
                await session.WriteTransactionAsync(async tx =>
                {
                    await tx.RunAsync(query, new {serverid1 = serverId1, serverid2});
                });
            }
            catch (Neo4jException exception)
            {
                await ReportError(query, exception.ToString());
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<List<IRecord>> GetAllNodesDirectionallyFromGivenNode(string rootNodeId)
        {
            const string query = @"
MATCH (from:Server {id:$rootNodeId})
CALL apoc.path.subgraphNodes(from, {labelFilter:'Server',relationshipFilter:'>'}) YIELD node
return node";

            var session = _driver.AsyncSession();
            try
            {
                var nodes = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new {rootNodeId});
                    return await result.ToListAsync();
                });
                return nodes;
            }
            catch (Neo4jException ex)
            {
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
                {
                    Content = $"[Neo4JException] {query} - {ex}"
                });
                Console.WriteLine($"{query} - {ex}");
                throw;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task CreateServerLink(string serverId1, string serverId2)
        {
            // To learn more about the Cypher syntax, see https://neo4j.com/docs/cypher-manual/current/
            // The Reference Card is also a good resource for keywords https://neo4j.com/docs/cypher-refcard/current/
            const string query = @"
        MERGE (s1:Server { id: $serverId1 })
        MERGE (s2:Server { id: $serverId2 })
        MERGE (s1)-[:LINKED_TO]->(s2)
        RETURN s1, s2";

            var session = _driver.AsyncSession();
            try
            {
                // Write transactions allow the driver to handle retries and transient error
                var writeResults = await session.WriteTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new {serverId1, serverId2});
                    return await result.ToListAsync();
                });
            }
            // Capture any errors along with the query and data for traceability
            catch (Neo4jException ex)
            {
                await Banlink.Hook.BroadcastMessageAsync(new DiscordWebhookBuilder
                {
                    Content = $"[Neo4JException] {query} - {ex}"
                });
                Console.WriteLine($"{query} - {ex}");
                Logger.Log(Logger.LogLevel.Fatal, $"Fatal error while creating link!\n{query}\n{ex}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<INode> FindServer(string id)
        {
            const string query = @"
        MATCH (s:Server)
        WHERE s.id = $id
        RETURN s";

            var session = _driver.AsyncSession();
            try
            {
                var readResults = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new {id});
                    return await result.ToListAsync();
                });

                var node = readResults[0].As<INode>();
                return node;
            }
            // Capture any errors along with the query and data for traceability
            catch (Neo4jException ex)
            {
                Console.WriteLine($"{query} - {ex}");
                throw;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing) _driver?.Dispose();

            _disposed = true;
        }
    }
}