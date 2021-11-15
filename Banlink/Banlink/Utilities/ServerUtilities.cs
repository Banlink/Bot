namespace Banlink.Utilities
{
    public static class ServerUtilities
    {
        public static bool IsInServer(string serverId)
        {
            // This may be critically inefficient when you have many servers.
            return serverId is {Length: 18} && Banlink.Client.Guilds.ContainsKey(ulong.Parse(serverId));
        }
    }
}