using Townsharp.Api;

namespace Townsharp.Servers
{
    // State pattern may still make sense here, with "Server" as the context object, and "ConnectedServer" etc. as a state object example
    public class Server
    {
        private readonly IApiClient apiClient;

        protected internal Server(ServerId id, IApiClient apiClient)
        {
            this.Id = id;
            this.apiClient = apiClient;
        }

        public bool IsOnline => this.apiClient.GetServerInfo(Id).Result.IsOnline;

        public ServerId Id { get; init; }

        public async Task<Player[]> GetCurrentPlayers()
        {
            var serverInfo = await this.apiClient.GetServerInfo(Id);

            return serverInfo.IsOnline ?
                serverInfo.OnlinePlayers
                    .Select(player => new Player(new PlayerId(player.Id), player.Username))
                    .ToArray() :
                new Player[0];
        }
    }
}