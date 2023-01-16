namespace Townsharp.Servers
{
    public record ServerStatus(bool IsOnline, PlayerInfo[] OnlinePlayers);
}
