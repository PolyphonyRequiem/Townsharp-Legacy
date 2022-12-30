using Moq;
using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Subscriptions;

namespace Test.Townsharp
{
    public class ServerTests
    {
        [Fact]
        public async Task TestServerFabrication()
        {
            var testServerId = 1;
            var sessionFactory = new TestSessionFactory(
                ()=>new Mock<GroupManager>().Object,
                ()=>new Mock<ServerManager>().Object,
                ()=>new Mock<SubscriptionService>().Object,
                ()=>new Mock<ConsoleSessionService>().Object);

            var session = sessionFactory.CreateConnectedSession(TestConfig.DefaultConfig);

            var joinedServers = await session.GetJoinedServersAsync();
            Assert.True(joinedServers.First().Id == testServerId);
        }
    }
}