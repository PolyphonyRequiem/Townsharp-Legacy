namespace Townsharp.Servers
{
    public interface IServerStatusProviderFactory
    {
        IServerStatusProvider CreateProviderForServerId(ServerId serverId);
    }
}