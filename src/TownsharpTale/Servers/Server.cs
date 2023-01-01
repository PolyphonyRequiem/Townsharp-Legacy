using System.Collections.ObjectModel;
using Townsharp.Groups;

namespace Townsharp.Servers
{
    public sealed class Server
    {
        private readonly Action<ServerOnlineEvent> onlineHandler;
        private readonly Action<ServerOfflineEvent> offlineHandler;
        private readonly Action<PlayerJoinedEvent> playerJoinedHandler;
        private readonly Action<PlayerLeftEvent> playerLeftHandler;
        private readonly IServerStatusProvider serverStatusProvider;
        private bool isOnline = false;
        private List<Player> lastOnlinePlayers = new List<Player>();

        public ServerId Id { get; }

        public GroupId GroupId { get; }

        public string Name { get; }

        public string Description { get; private set; }

        public string Region { get; }

        // this is VERY inefficient, cache this and invalidate it if the players change.
        public ReadOnlyDictionary<PlayerId, Player> Players => lastOnlinePlayers.ToDictionary(p => p.Id, p => p).AsReadOnly();

        private Server(
            ServerDescription description,
            Action<ServerOnlineEvent> onlineHandler,
            Action<ServerOfflineEvent> offlineHandler,
            Action<PlayerJoinedEvent> playerJoinedHandler,
            Action<PlayerLeftEvent> playerLeftHandler,
            IServerStatusProvider serverStatusProvider)
        {
            this.Id = description.Id;
            this.GroupId = description.GroupId;
            this.Name = description.Name;
            this.Description = description.Description;
            this.Region = description.Region;

            this.onlineHandler = onlineHandler;
            this.offlineHandler = offlineHandler;
            this.playerJoinedHandler = playerJoinedHandler;
            this.playerLeftHandler = playerLeftHandler;

            this.serverStatusProvider = serverStatusProvider;
        }

        internal static async Task<Server> CreateServerAsync(
            ServerDescription description,
            Action<ServerOnlineEvent> onlineHandler,
            Action<ServerOfflineEvent> offlineHandler,
            Action<PlayerJoinedEvent> playerJoinedHandler,
            Action<PlayerLeftEvent> playerLeftHandler,
            IServerStatusProvider serverStatusProvider)
        {
            var server = new Server(
                description,
                onlineHandler,
                offlineHandler,
                playerJoinedHandler,
                playerLeftHandler,
                serverStatusProvider);

            await server.StartManagementAsync();

            return server;
        }
        public async Task<bool> CheckIsServerOnlineAsync() => await this.serverStatusProvider.CheckIsServerOnlineAsync();

        public async Task<PlayerDescription[]> GetCurrentPlayerDescriptionsAsync() => await this.serverStatusProvider.GetCurrentPlayerDescriptionsAsync();

        public async Task RefreshStatus()
        {
            var status = await this.serverStatusProvider.GetStatusAsync();
            HandleStatusChange(status);
        }

        private async Task StartManagementAsync()
        {
            this.serverStatusProvider.RegisterStatusChangeHandler(HandleStatusChange);
            await RefreshStatus();
        }
        
        private void HandleStatusChange(ServerStatus status)
        {
            var currentPlayerIds = this.lastOnlinePlayers.Select(p => p.Id);
            var updatedPlayerIds = status.OnlinePlayers.Select(p => p.Id);

            if (!updatedPlayerIds.SequenceEqual(currentPlayerIds))
            {
                // handle players changed.
                var playersJoinedIds = updatedPlayerIds.Except(currentPlayerIds).ToArray();
                var playersLeftIds = currentPlayerIds.Except(updatedPlayerIds).ToArray();

                var lastKnownPlayersMap = this.lastOnlinePlayers.ToDictionary(p => p.Id, p => p);

                foreach (var leavingPlayerId in playersLeftIds)
                {
                    var leavingPlayer = lastKnownPlayersMap[leavingPlayerId];
                    lastOnlinePlayers.Remove(leavingPlayer);
                    this.playerLeftHandler.Invoke(new PlayerLeftEvent(leavingPlayer));
                }

                var onlinePlayersMap = status.OnlinePlayers.ToDictionary(p => p.Id, p => p);

                foreach (var joiningPlayerId in playersJoinedIds)
                {
                    var joiningPlayer = new Player(joiningPlayerId, onlinePlayersMap[joiningPlayerId].UserName);
                    lastOnlinePlayers.Add(joiningPlayer);
                    this.playerJoinedHandler.Invoke(new PlayerJoinedEvent(joiningPlayer));
                }
            }

            if (status.IsOnline != this.isOnline)
            {
                // handle online status changed.
                if (status.IsOnline)
                {
                    this.onlineHandler(new ServerOnlineEvent());
                }
                else
                {
                    this.offlineHandler(new ServerOfflineEvent());
                }
            }
        }

        public record struct ServerOnlineEvent();

        public record struct ServerOfflineEvent();

        public record struct PlayerJoinedEvent(Player JoiningPlayer);

        public record struct PlayerLeftEvent(Player LeavingPlayer);
    }
}