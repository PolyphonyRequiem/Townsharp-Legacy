namespace Townsharp.Servers
{
    // State pattern may still make sense here, with "Server" as the context object, and "ConnectedServer" etc. as a state object example
    public abstract class Server
    {
        protected Server(ServerId id, ServersManager serverManager)
        {
            this.Id = id;
            this.ServersManager = serverManager;
        }

        protected internal ServersManager ServersManager { get; init; }

        public bool IsOnline { get; set; }

        public ServerId Id { get; init; }

        public abstract Task<Player[]> GetCurrentPlayers();
    }
}