using Townsharp.Groups;

namespace Townsharp.Servers
{
    public record struct ServerDescription(
        ServerId Id,
        GroupId GroupId,
        string Name,
        string Description,
        string Region);
}
