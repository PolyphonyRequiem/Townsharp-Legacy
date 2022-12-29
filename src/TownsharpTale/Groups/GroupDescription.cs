using Townsharp.Servers;

namespace Townsharp.Groups
{
    public record struct GroupDescription(
        GroupId Id, 
        string Name,
        string Description,
        GroupType GroupType);
}
