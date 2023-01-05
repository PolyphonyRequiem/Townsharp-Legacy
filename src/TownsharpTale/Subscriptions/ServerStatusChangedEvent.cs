using Townsharp.Groups;
using Townsharp.Servers;

namespace Townsharp.Subscriptions
{
    public record struct ServerStatusChangedEvent (ServerId ServerId, GroupId GroupId, bool IsOnline, PlayerInfo[] OnlinePlayers);        
}
