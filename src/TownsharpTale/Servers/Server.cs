namespace Townsharp.Servers
{
    public class Server
    {
        private readonly IServerUpdateProvider serverUpdateProvider;

        protected Server(ServerId serverId, ServerStatus serverStatus, IServerUpdateProvider serverStatusProvider)
        {
            this.ServerId = serverId;
            this.ServerStatus = serverStatus;
            this.serverUpdateProvider = serverStatusProvider;
            this.serverUpdateProvider.SubscribeForStatusUpdates(Apply);
        }

        public Task<ServerConnection> Connect()
        {
            throw new NotImplementedException();
        }

        public ServerId ServerId { get; }

        public ServerStatus ServerStatus { get; }

        public ServerConnection ServerConnection { get; }

        protected void Apply(ServerStatusUpdate update)
        {
            throw new NotImplementedException();
        }        
    }
}