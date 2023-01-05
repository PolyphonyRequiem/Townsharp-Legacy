using Townsharp.Servers;

namespace Townsharp.Groups
{
    public record struct GroupInfo(
        GroupId Id, 
        string Name,
        string Description,
        GroupType GroupType);
}
