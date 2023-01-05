using Townsharp.Groups;

namespace Townsharp.Servers
{
    public class Server
    {
        private readonly ServerManager serverManager;

        public ServerId Id { get; }

        public GroupId GroupId { get; }

        public string Name { get; }

        public string Description { get; private set; }

        public string Region { get; }

        private Server(
            ServerInfo description,
            ServerManager serverManager)
        {
            this.Id = description.Id;
            this.GroupId = description.GroupId;
            this.Name = description.Name;
            this.Description = description.Description;
            this.Region = description.Region;

            this.serverManager = serverManager;
        }

        internal static Server Create(
            ServerInfo description,
            ServerManager serverManager)
        {
            return new Server(
                description,
                serverManager);
        }

        public async Task<bool> CheckIsOnlineAsync() => await this.serverManager.GetUpdatedServerOnlineStatusAsync();

        public async Task<PlayerInfo[]> GetOnlinePlayers() => await this.serverManager.GetOnlinePlayersAsync(this);
    }
}