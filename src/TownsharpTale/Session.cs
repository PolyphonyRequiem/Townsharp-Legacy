using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Townsharp.Api;
using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Townsharp
{
    public class Session
    {
        private readonly Dictionary<GroupId, Group> JoinedGroups = new Dictionary<GroupId, Group>();
        private readonly Dictionary<ServerId, Server> JoinedServers = new Dictionary<ServerId, Server>();

        private readonly TownsharpConfig config;
        private readonly IApiClient apiClient;
        private readonly ISubscriptionClient subscriptionClient;
        private readonly IConsoleClientFactory consoleClientFactory;

        public Session(TownsharpConfig config,
            IApiClient apiClient,
            ISubscriptionClient subscriptionClient,
            IConsoleClientFactory consoleClientFactory)
        {
            this.config = config;
            this.apiClient = apiClient;
            this.subscriptionClient = subscriptionClient;
            this.consoleClientFactory = consoleClientFactory;
        }

        public Task<ReadOnlyCollection<Server>> GetJoinedServers()
        {
            return Task.FromResult(JoinedServers.Values.ToList().AsReadOnly());
        }

        private async Task InitializeSessionAsync()
        {
            if (this.config.AutoManageJoinedGroups == true) 
            {
                List<Task> groupManagementInitializationTasks = new List<Task>();
                await foreach (var joinedGroupDescriptor in this.apiClient.GetJoinedGroups())
                {
                    var joinedGroup = new Group(joinedGroupDescriptor);
                    groupManagementInitializationTasks.Add(this.ManageGroup(joinedGroup));
                }
            }
        }

        private Task ManageGroup(Group joinedGroup)
        {
            throw new NotImplementedException();
        }

        // get notified on invitations

        // subscribe to events at appropriate scopes

        // So let's distinguish servers from managed servers, consoles from managed consoles, and groups from managed groups.
        // You can query/access servers/groups/consoles, and even do tasks on then, but in practice it's not much more than a dumb state object that only updates in response to events.
        // Any state that could be volatile should be part of a "snapshot" of some sort to make it clear that the state is a snapshot and not a fixed thing.
        // Managed servers etc should have state exposed in a more "continuous" way, and we should even expose "last updated" semantics at whatever scope.
    }
}