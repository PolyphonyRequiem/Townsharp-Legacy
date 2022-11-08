namespace Townsharp.Servers
{
    public interface IServerUpdateProvider
    {
        public Task<ServerStatus> GetStatusAsync();

        public void SubscribeForStatusUpdates(Action<ServerStatusUpdate> updateHandler, CancellationToken cancellationToken=default);
    }
}