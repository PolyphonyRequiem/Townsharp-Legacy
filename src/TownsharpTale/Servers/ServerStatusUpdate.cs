namespace Townsharp.Servers
{
    public record struct ServerStatusUpdate
    {
        public ServerStatus NewStatus { get; init; }
    }
}