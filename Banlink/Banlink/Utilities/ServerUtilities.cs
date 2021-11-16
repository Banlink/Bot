using System;

namespace Banlink.Utilities
{
    public static class ServerUtilities
    {
        public static bool IsInServer(string serverId)
        {
            // This may be critically inefficient when you have many servers.
            return serverId is {Length: 18} && Banlink.Client.Guilds.ContainsKey(ulong.Parse(serverId));
        }

        // Cryptographically good enough :tm:
        public static string GenerateLinkCode(string serverId)
        {
            var guid = Guid.NewGuid().ToString().Split("-")[0].Replace("-", "");
            var code = serverId + "=" + guid;
            return code;
        }
    }
}