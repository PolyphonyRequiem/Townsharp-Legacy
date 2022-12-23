namespace Townsharp.Servers
{
    public enum ServerStatusChangeEvents
    {
        ServerOnlineStatusChange,
        ServerOnlinePlayersChange
    }

    public abstract record ServerStatusChange (ServerStatusChangeEvents EventType);

    public record ServerOnlineStatusChanged (bool IsOnline) : ServerStatusChange (ServerStatusChangeEvents.ServerOnlineStatusChange);

    public record ServerPlayersChanged(Player[] currentPlayers, Player[] playersJoined, Player[] playersLeft) : ServerStatusChange(ServerStatusChangeEvents.ServerOnlinePlayersChange);
}