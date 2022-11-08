using MediatR;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Servers.Consoles;
using Townsharp.Subscriptions;

namespace Townsharp
{
    public class Session
    {
        private readonly TownsharpConfig config;

        private readonly Dictionary<GroupId, Group> groups = new Dictionary<GroupId, Group>(); 
        private readonly Dictionary<ServerId, Server> servers = new Dictionary<ServerId, Server>();
        private readonly Dictionary<SubscriptionId, Subscription> subscriptions = new Dictionary<SubscriptionId, Subscription>();
        private readonly Dictionary<ServerId, ConsoleSession> consoleSessions = new Dictionary<ServerId, ConsoleSession>();

        public event Action<SessionReadyEvent>? SessionReady;

        public Session(TownsharpConfig config)
        {
            this.config = config;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            // Get all known information about groups and servers, register for standard subscriptions.
            this.SessionReady?.Invoke(SessionReadyEvent.Default);
            await Task.CompletedTask;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await Start(cancellationToken);
            cancellationToken.WaitHandle.WaitOne();
        }        
    }
}
