namespace Townsharp.Servers
{
    public class Server
    {
        protected Server(ServerId serverId, ServerStatus serverStatus)
        {
            this.ServerId = serverId;
            this.ServerStatus = serverStatus;
        }

        public ServerId ServerId { get; }

        public ServerStatus ServerStatus { get; set; }06
    }
}