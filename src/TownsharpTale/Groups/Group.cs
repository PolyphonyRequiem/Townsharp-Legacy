using Townsharp.Servers;

namespace Townsharp.Groups
{
    public class Group
    {
        public GroupId Id { get; init; }

        public GroupManager GroupManager { get; init; }
               
        protected Group(GroupId id, GroupManager groupManager)
        {
            this.Id = id;
            GroupManager = groupManager;
        }

        // groups can be in a few states at least:
        // Forbidden
        // Doesn't exist (404)
        // Accessible
        // Any difference between invited and joined?  Get help investigating that and record it.
        // Might need a state field here, possibly with "As" methods to promote the view if aligned (this can be extended by the implementation factories)
    }
}
