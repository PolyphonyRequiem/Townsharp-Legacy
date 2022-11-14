using System.Collections.ObjectModel;
using Townsharp.Groups;

namespace Townsharp
{
    public interface IGroupsManager
    {
        public ReadOnlyDictionary<GroupId, Group> GetJoinedGroups();
    }
}
