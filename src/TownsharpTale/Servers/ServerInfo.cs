using Townsharp.Groups;

namespace Townsharp.Servers
{
    public record struct ServerInfo(
        ServerId Id,
        GroupId GroupId,
        string Name,
        string Description,
        string Region);
}
