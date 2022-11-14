using System.Collections.ObjectModel;
using Townsharp.Groups;
using Townsharp.Servers;

namespace Townsharp
{
    public class Session
    {
        private readonly TownsharpConfig config;
        private readonly IGroupsManager groupsManager;
        private readonly IServerManager serverManager;

        public Session(TownsharpConfig config, IGroupsManager groupsManager, IServerManager serverManager)
        {
            this.config = config;
            this.groupsManager = groupsManager;
            this.serverManager = serverManager;
        }

        public ReadOnlyDictionary<GroupId, Group> JoinedGroups => this.groupsManager.GetJoinedGroups();

        public Task<Server> ManageServer(ServerId serverId)
        {
            return this.serverManager.RegisterServerForManagement(serverId);
        }

        // get notified on invitations

        // subscribe to events at appropriate scopes

        // So let's distinguish servers from managed servers, consoles from managed consoles, and groups from managed groups.
        // You can query/access servers/groups/consoles, and even do tasks on then, but in practice it's not much more than a dumb state object that only updates in response to events.
        // Any state that could be volatile should be part of a "snapshot" of some sort to make it clear that the state is a snapshot and not a fixed thing.
        // Managed servers etc should have state exposed in a more "continuous" way, and we should even expose "last updated" semantics at whatever scope.
    }
}
