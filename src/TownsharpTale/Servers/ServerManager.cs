using System.Collections.Concurrent;
using static Townsharp.Servers.Server;

namespace Townsharp.Servers
{
    public class ServerManager
    {
        private readonly TownsharpConfig config;

        private readonly ConcurrentDictionary<ServerId, Server> managedServers = new ConcurrentDictionary<ServerId, Server>();

        // Hey, I'm pretty sure we can make this sealed, and then let the Session create it and handle it.  Check factory pattern for status provider.
        public ServerManager(TownsharpConfig config)
        {
            this.config = config;
        }

        public Server AddServer(ServerInfo serverDescription)
        {
            var serverId = serverDescription.Id;

            if (this.managedServers.TryGetValue(serverId, out var server))
            {
                return server;
            }

            var newServer = Server.Create(serverDescription, this);

            var finalServer = this.managedServers.AddOrUpdate(serverId, newServer, (id, s) => s);

            return finalServer;
        }

        private void OnlineHandler(ServerId serverId, ServerOnlineEvent serverOnlineEvent)
        {
            throw new NotImplementedException();
        }

        private void OfflineHandler(ServerId serverId, ServerOfflineEvent serverOfflineEvent)
        {
            throw new NotImplementedException();
        }

        private void PlayerJoinedHandler(ServerId serverId, PlayerJoinedEvent playerJoinedEvent)
        {
            throw new NotImplementedException();
        }

        private void PlayerLeftHandler(ServerId serverId, PlayerLeftEvent playerLeftEvent)
        {
            throw new NotImplementedException();
        }

        internal Task<PlayerInfo[]> GetOnlinePlayersAsync(Server server)
        {
            throw new NotImplementedException();
        }

        internal Task<bool> GetUpdatedServerOnlineStatusAsync()
        {
            throw new NotImplementedException();
        }

        //private void HandleStatusChange(ServerStatus status)
        //{
        //    var currentPlayerIds = this.lastOnlinePlayers.Select(p => p.Id);
        //    var updatedPlayerIds = status.OnlinePlayers.Select(p => p.Id);

        //    if (!updatedPlayerIds.SequenceEqual(currentPlayerIds))
        //    {
        //        // handle players changed.
        //        var playersJoinedIds = updatedPlayerIds.Except(currentPlayerIds).ToArray();
        //        var playersLeftIds = currentPlayerIds.Except(updatedPlayerIds).ToArray();

        //        var lastKnownPlayersMap = this.lastOnlinePlayers.ToDictionary(p => p.Id, p => p);

        //        foreach (var leavingPlayerId in playersLeftIds)
        //        {
        //            var leavingPlayer = lastKnownPlayersMap[leavingPlayerId];
        //            lastOnlinePlayers.Remove(leavingPlayer);
        //            this.playerLeftHandler.Invoke(new PlayerLeftEvent(leavingPlayer));
        //        }

        //        var onlinePlayersMap = status.OnlinePlayers.ToDictionary(p => p.Id, p => p);

        //        foreach (var joiningPlayerId in playersJoinedIds)
        //        {
        //            var joiningPlayer = new Player(joiningPlayerId, onlinePlayersMap[joiningPlayerId].UserName);
        //            lastOnlinePlayers.Add(joiningPlayer);
        //            this.playerJoinedHandler.Invoke(new PlayerJoinedEvent(joiningPlayer));
        //        }
        //    }

        //    if (status.IsOnline != this.isOnline)
        //    {
        //        // handle online status changed.
        //        if (status.IsOnline)
        //        {
        //            this.onlineHandler(new ServerOnlineEvent());
        //        }
        //        else
        //        {
        //            this.offlineHandler(new ServerOfflineEvent());
        //        }
        //    }
        //}

        public record struct ServerOnlineEvent();

        public record struct ServerOfflineEvent();

        public record struct PlayerJoinedEvent(Player JoiningPlayer);

        public record struct PlayerLeftEvent(Player LeavingPlayer);
    }
}