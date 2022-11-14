namespace Townsharp.Servers
{
    public interface IServerManager
    {
        Task<Server> RegisterServerForManagement(ServerId serverId);
    }
}
