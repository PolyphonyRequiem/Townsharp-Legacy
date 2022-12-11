using Townsharp.Infra.Alta.Api;
using Townsharp.Servers;

namespace Townsharp.Infra
{
    internal class AltaServer : Server
    {
        private readonly ApiClient apiClient;

        protected internal AltaServer(ServerId id, ServersManager serversManager, ApiClient apiClient, bool isOnline)
            : base(id, serversManager)
        {
            this.IsOnline = isOnline;
            this.apiClient = apiClient;
        }

        public async override Task<Player[]> GetCurrentPlayers()
        {
            var serverInfo = await this.apiClient.GetServerInfo(base.Id);

            return serverInfo.IsOnline ?
                serverInfo.OnlinePlayers
                    .Select(player => new Player(new PlayerId(player.Id), player.Username))
                    .ToArray() :
                new Player[0];
        }
    }
}