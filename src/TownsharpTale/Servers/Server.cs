using Townsharp.Api;
using Townsharp.Groups;

namespace Townsharp.Servers
{
    public class Server
    {
        private readonly ApiClient apiClient;

        public ServerId Id { get; }

        public GroupId GroupId { get; }

        public string Name { get; }

        public string Description { get; private set; }

        public string Region { get; }

        private Server(
            ServerId id,
            GroupId groupId,
            string name,
            string description,
            string region,
            ApiClient apiClient)
        {
            this.Id = id;
            this.GroupId = groupId;
            this.Name = name;
            this.Description = description;
            this.Region = region;
            this.apiClient = apiClient;
        }

        internal static Server Create(
            ServerId id,
            GroupId groupId,
            string name,
            string description,
            string region,
            ApiClient apiClient)
        {
            return new Server(
                id,
                groupId, 
                name,
                description,
                region,
                apiClient);
        }

        public async Task<bool> CheckIsOnlineAsync() => (await this.apiClient.GetServerAsync(this.Id)).IsOnline;


        public async Task<IEnumerable<PlayerInfo>> GetOnlinePlayers() => 
            (await this.apiClient.GetServerAsync(this.Id))
                .OnlinePlayers
                .Select(player=> new PlayerInfo(player.Id, player.Username));
    }
}