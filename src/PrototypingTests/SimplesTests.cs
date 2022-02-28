using Xunit;
using System.Net.WebSockets;

namespace PrototypingTests
{
    public class SimpleTests
    {
        [Fact]
        public async Task CreateAuthenticatedClientTest()
        {
            var client = await ApiClient.CreateAuthenticatedClientAsync();

            Assert.True(client != null);
        }
    }
}