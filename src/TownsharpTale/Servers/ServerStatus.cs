namespace Townsharp.Servers
{
    public record struct ServerStatus(bool IsOnline, PlayerDescription[] OnlinePlayers);
}
