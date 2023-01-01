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
            var sessionFactory = new SessionFactory<TestSession>(TestSession.Create);

            var session = sessionFactory.Create(TownsharpConfig.Default);

            var joinedServers = await session.GetJoinedServerDescriptionsAsync();

            Assert.True(joinedServers.First().Id == testServerId);
        }
    }
}