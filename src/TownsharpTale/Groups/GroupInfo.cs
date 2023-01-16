using Townsharp.Servers;

namespace Townsharp.Groups
{
    public record GroupInfo(
        GroupId Id, 
        string Name,
        string Description,
        GroupType GroupType);
}
