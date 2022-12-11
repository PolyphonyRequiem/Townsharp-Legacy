using Townsharp.Infra.Alta.Api;
using Townsharp.Servers;

namespace Townsharp.Infra
{
    public class AltaServersManager : ServersManager
    {
        private readonly ApiClient apiClient;

        public AltaServersManager(
            ApiClient apiClient
            // ServerFactory ?
            )
        {
            this.apiClient = apiClient;
        }

        public async override Task<ServerId[]> GetJoinedServerIds()
        {
            var joinedGroups = await apiClient.GetJoinedGroups();
            
            var joinedServerIds = joinedGroups
                .Select(joinedGroup => joinedGroup.Group)
                .SelectMany(group => group.Servers)
                .Select(server => new ServerId(server.Id))
                .ToArray();

            return joinedServerIds;
        }

        public async override Task<Server> GetServer(ServerId id)
        {
            // This logic seems like a universal invariant, and should likely be abstracted into ServersManager.
            if (this.ManagedServers.ContainsKey(id))
            {
                return this.ManagedServers[id];
            }
            ///////////////////////////////

            var serverInfo = await this.apiClient.GetServerInfo(id);

            // clearly needs a factory
            return new AltaServer(id, this, this.apiClient, serverInfo.IsOnline);
        }
    }
}
