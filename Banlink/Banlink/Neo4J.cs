using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Banlink.Utilities;
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Neo4J()
        {
            Dispose(false);
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
                Console.WriteLine($"{query} - {ex}");
                throw;
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
                Console.WriteLine($"{query} - {ex}");
                Logger.Log(Logger.LogLevel.Fatal, $"Fatal error while creating link!\n{query}\n{ex}");
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task FindServer(string id)
        {
            const string query = @"
        MATCH (s:Server)
        WHERE s.id = $id
        RETURN s.id";

            var session = _driver.AsyncSession();
            try
            {
                var readResults = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new {id});
                    return await result.ToListAsync();
                });

                foreach (var result in readResults) Console.WriteLine($"Found server: {result["s.id"].As<string>()}");
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