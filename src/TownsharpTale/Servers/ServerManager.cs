using System.Collections.Concurrent;
using Townsharp.Api;
using Townsharp.Subscriptions;

namespace Townsharp.Servers
{
    /// <summary>
    /// Responsible for Managing Server Lifecycles, responding to subscription events, 
    /// </summary>
    public class ServerManager
    {
        private readonly ConcurrentDictionary<ServerId, Server> managedServers = new ConcurrentDictionary<ServerId, Server>();
        private readonly ApiClient apiClient;
        private readonly SubscriptionService subscriptionService;

        internal ServerManager(
            ApiClient apiClient,
            SubscriptionService subscriptionService)
        {
            this.apiClient = apiClient;
            this.subscriptionService = subscriptionService;
        }

        public async Task<Server> ManageServerAsync(ServerId serverId)
        {
            if (this.managedServers.TryGetValue(serverId, out var server))
            {
                return server;
            }

            var response = await this.apiClient.GetServerAsync(serverId);

            var newServer = Server.Create(
                response.Id,
                response.GroupId,
                response.Name,
                response.Description,
                response.Region,
                this.apiClient);

            var finalServer = this.managedServers.AddOrUpdate(serverId, newServer, (id, s) => s);

            return finalServer;
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