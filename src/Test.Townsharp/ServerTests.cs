using Moq;
using Townsharp.Api;
using Townsharp.Consoles;
using Townsharp.Subscriptions;

namespace Test.Townsharp
{
    public class ServerTests
    {
        [Fact]
        public async Task TestServerFabrication()
        {
            var mockApiClient = new Mock<IApiClient>();
            var mockSubscriptionClient = new Mock<ISubscriptionClient>();
            var mockConsoleClientFactory = new Mock<IConsoleClientFactory>();

            var session = new Session(
                TestConfig.DefaultConfig, 
                mockApiClient.Object, 
                mockSubscriptionClient.Object, 
                mockConsoleClientFactory.Object );

            var joinedServers = await session.GetJoinedServers();
            Assert.True(joinedServers.First().IsOnline);
        }
    }
}