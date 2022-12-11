using System.Collections.ObjectModel;

namespace Townsharp.Groups
{
    public abstract class GroupsManager
    {
        public abstract Task<GroupId[]> GetJoinedGroups();
    }
}
