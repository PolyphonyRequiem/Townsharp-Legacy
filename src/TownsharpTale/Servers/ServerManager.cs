using System.Collections.Concurrent;
using static Townsharp.Servers.Server;

namespace Townsharp.Servers
{
    public abstract class ServerManager
    {
        private readonly Action<Server, ServerOnlineEvent> onlineHandler;
        private readonly Action<Server, ServerOfflineEvent> offlineHandler;
        private readonly Action<Server, PlayerJoinedEvent> playerJoinedHandler;
        private readonly Action<Server, PlayerLeftEvent> playerLeftHandler;

        private readonly IServerStatusProviderFactory serverStatusProviderFactory;

        private readonly Dictionary<ServerId, Action<ServerOnlineEvent>> onlineHandlers = new();
        private readonly Dictionary<ServerId, Action<ServerOfflineEvent>> offlineHandlers = new();
        private readonly Dictionary<ServerId, Action<PlayerJoinedEvent>> playerJoinedHandlers = new();
        private readonly Dictionary<ServerId, Action<PlayerLeftEvent>> playerLeftHandlers = new();

        private readonly TownsharpConfig config;

        public ConcurrentDictionary<ServerId, Server> Servers { get; set; } = new ConcurrentDictionary<ServerId, Server>();

        public ServerManager(
            TownsharpConfig config,
            Action<Server, ServerOnlineEvent> onlineHandler,
            Action<Server, ServerOfflineEvent> offlineHandler,
            Action<Server, PlayerJoinedEvent> playerJoinedHandler,
            Action<Server, PlayerLeftEvent> playerLeftHandler,
            IServerStatusProviderFactory serverStatusProviderFactory)
        {
            this.config = config;
            this.onlineHandler = onlineHandler;
            this.offlineHandler = offlineHandler;
            this.playerJoinedHandler = playerJoinedHandler;
            this.playerLeftHandler = playerLeftHandler;
            this.serverStatusProviderFactory = serverStatusProviderFactory;
        }

        public async Task<Server> ManageServerAsync(ServerId serverId) => await GetServerCoreAsync(serverId, true);

        public async Task<Server> GetServerAsync(ServerId serverId) => await GetServerCoreAsync(serverId, this.config.AutoManageJoinedServers);

        public async Task<Server> GetServerCoreAsync(ServerId serverId, bool manageServer)
        {
            if (this.Servers.TryGetValue(serverId, out var server))
            {
                return server;
            }

            var serverDescription = await GetServerDescriptionAsync(serverId);

            var onlineHandler = (ServerOnlineEvent e) => this.OnlineHandler(serverId, e);
            var offlineHandler = (ServerOfflineEvent e) => this.OfflineHandler(serverId, e);
            var playerJoinedHandler = (PlayerJoinedEvent e) => this.PlayerJoinedHandler(serverId, e);
            var playerLeftHandler = (PlayerLeftEvent e) => this.PlayerLeftHandler(serverId, e);

            IServerStatusProvider serverStatusProvider = serverStatusProviderFactory.CreateProviderForServerId(serverId);

            var newServer = Server.CreateServer(serverDescription, onlineHandler, offlineHandler, playerJoinedHandler, playerLeftHandler, serverStatusProvider);

            var finalServer = this.Servers.AddOrUpdate(serverId, newServer, (id, s) => s);

            onlineHandlers.TryAdd(serverId, onlineHandler);
            offlineHandlers.TryAdd(serverId, offlineHandler);
            playerJoinedHandlers.TryAdd(serverId, playerJoinedHandler);
            playerLeftHandlers.TryAdd(serverId, playerLeftHandler);

            if (this.config.AutoManageJoinedServers)
            {
                await finalServer.StartManagementAsync();
            }

            return finalServer;
        }

        // do these need to be here and abstract? or should they be on some sort of dependency?
        public abstract Task<ServerDescription> GetServerDescriptionAsync(ServerId serverId);

        public abstract Task<Server[]> GetJoinedServersAsync();

        private void OnlineHandler(ServerId serverId, Server.ServerOnlineEvent serverOnlineEvent)
        {
            if (this.onlineHandlers.TryGetValue(serverId, out var onlineHandler))
            {
                onlineHandler.Invoke(serverOnlineEvent);
            }
            else
            {
                throw new InvalidOperationException("Possible race condition. This isn't expected, please notify Townsharp Developers.");
            }
        }

        private void OfflineHandler(ServerId serverId, Server.ServerOfflineEvent serverOfflineEvent)
        {
            if (this.offlineHandlers.TryGetValue(serverId, out var offlineHandler))
            {
                offlineHandler.Invoke(serverOfflineEvent);
            }
            else
            {
                throw new InvalidOperationException("Possible race condition. This isn't expected, please notify Townsharp Developers.");
            }
        }

        private void PlayerJoinedHandler(ServerId serverId, Server.PlayerJoinedEvent playerJoinedEvent)
        {
            if (this.playerJoinedHandlers.TryGetValue(serverId, out var playerJoinedHandler))
            {
                playerJoinedHandler.Invoke(playerJoinedEvent);
            }
            else
            {
                throw new InvalidOperationException("Possible race condition. This isn't expected, please notify Townsharp Developers.");
            }
        }

        private void PlayerLeftHandler(ServerId serverId, Server.PlayerLeftEvent playerLeftEvent)
        {
            if (this.playerLeftHandlers.TryGetValue(serverId, out var playerLeftHandler))
            {
                playerLeftHandler.Invoke(playerLeftEvent);
            }
            else
            {
                throw new InvalidOperationException("Possible race condition. This isn't expected, please notify Townsharp Developers.");
            }
        }
    }
}