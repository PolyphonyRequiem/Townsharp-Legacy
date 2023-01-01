using Moq;
using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Test.Townsharp
{
    internal class TestSession : Session
    {
        internal TestSession(
            TownsharpConfig config,
            GroupManager groupManager,
            ServerManager serverManager,
            SubscriptionService subscriptionService,
            ConsoleSessionService consoleSessionService)
            : base(config, groupManager, serverManager, subscriptionService, consoleSessionService)
        {

        } 

        internal static TestSession Create(TownsharpConfig config)
        {
            return new TestSession(config,
                new Mock<GroupManager>().Object,
                new Mock<ServerManager>().Object,
                new Mock<SubscriptionService>().Object,
                new Mock<ConsoleSessionService>().Object);
        }

        public override Task<ServerDescription[]> GetJoinedServerDescriptionsAsync()
        {
            return Task.FromResult(new ServerDescription[]
            {
                new ServerDescription(1, 1, "TestServer", "Used in Testing", "Test Region")
            });
        }

        public override Task<ServerDescription> GetServerDescriptionAsync(ServerId serverId)
        {
            return Task.FromResult(new ServerDescription(1, 1, "TestServer", "Used in Testing", "Test Region"));
        }

        protected override Task OnShutdownAsync()
        {
            // shutting down
            return Task.CompletedTask;
        }

        protected override Task OnStartAsync()
        {
            // starting up!

            return Task.CompletedTask;
        }
    }
}