namespace Townsharp.Servers
{
    public class ManagedServer : Server
    {
        public ManagedServer(ServerId serverId, ServerStatus serverStatus, IServerUpdateProvider serverStatusProvider)
            : base(serverId, serverStatus)
        {
            serverStatusProvider.SubscribeForStatusUpdates(update => this.ServerStatus = update.NewStatus);
        }
    }
}
