using System;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Banlink
{
    public sealed class Neo4J : IDisposable
    {
        private bool _disposed = false;
        private readonly IDriver _driver;

        ~Neo4J() => Dispose(false);

        public Neo4J(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
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
                await session.WriteTransactionAsync(async tx =>
                {
                    await tx.RunAsync(query, new {id = serverId});
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

        public async Task CreateServerLink(string serverId1, string serverId2)
        {
            // To learn more about the Cypher syntax, see https://neo4j.com/docs/cypher-manual/current/
            // The Reference Card is also a good resource for keywords https://neo4j.com/docs/cypher-refcard/current/
            var query = @"
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
                    return (await result.ToListAsync());
                });

                foreach (var result in writeResults)
                {
                    var server1 = result["s1"].As<INode>().Properties["id"];
                    var server2 = result["s2"].As<INode>().Properties["id"];
                    Console.WriteLine($"Created friendship between: {serverId1}, {serverId2}");
                }
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

        public async Task FindServer(string id)
        {
            var query = @"
        MATCH (s:Server)
        WHERE s.id = $id
        RETURN s.id";

            var session = _driver.AsyncSession();
            try
            {
                var readResults = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, new {id = id});
                    return (await result.ToListAsync());
                });

                foreach (var result in readResults)
                {
                    Console.WriteLine($"Found server: {result["s.id"].As<String>()}");
                }
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _driver?.Dispose();
            }

            _disposed = true;
        }
    }
}