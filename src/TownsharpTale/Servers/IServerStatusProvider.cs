using Townsharp.Subscriptions;

namespace Townsharp.Servers
{
    public interface IServerStatusProvider
    {
        void RegisterStatusChangeHandler(Action<ServerStatus> value);

        Task<bool> CheckIsServerOnlineAsync();

        Task<PlayerDescription[]> GetCurrentPlayerDescriptionsAsync();

        Task<ServerStatus> GetStatusAsync();
    }
}