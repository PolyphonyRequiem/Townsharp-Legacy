using Townsharp.Api;

namespace Townsharp.Servers
{
    public class ServerManager
    {
        private readonly IApiClient apiClient;

        // Concept of "managed" is still fuzzy here, define in the ubiquitous language in obsidian at some point please.
        private readonly Dictionary<ServerId, Server> KnownServers = new Dictionary<ServerId, Server>();
        private readonly Dictionary<ServerId, Server> ManagedServers = new Dictionary<ServerId, Server>();

        public ServerManager(
            IApiClient apiClient
            // and notification sources, probably an interface?
            )
        {
            this.apiClient = apiClient;
        }


        public async Task<ServerId[]> GetJoinedServerIds()
        {
            var joinedGroups = await apiClient.GetJoinedGroups();

            var joinedServerIds = joinedGroups
                .Select(joinedGroup => joinedGroup.Group)
                .SelectMany(group => group.Servers)
                .Select(server => new ServerId(server.Id))
                .ToArray();

            return joinedServerIds;
        }

        public async Task<Server> GetServer(ServerId id)
        {
            // This logic seems like a universal invariant, and should likely be abstracted into ServersManager.
            if (this.ManagedServers.ContainsKey(id))
            {
                return this.ManagedServers[id];
            }
            ///////////////////////////////

            var serverInfo = await this.apiClient.GetServerInfo(id);

            // clearly needs a factory
            return new Server(id, this.apiClient);
        }
    }
}
