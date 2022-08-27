using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Townsharp;

namespace Test.Townsharp
{
    public class SessionTests
    {
        [Fact]
        public async Task HandleSessionReadyEventAsynchronously()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMediatR(typeof(Session));
                });

            await builder.StartAsync();

            var session = new Session(TestConfig.DefaultConfig);

            Action<SessionReadyEvent> handleSessionReady = e => { };
            SessionReadyEvent? sessionReadyEvent = null;
            handleSessionReady = e =>
            {
                sessionReadyEvent = e;
                session.SessionReady -= handleSessionReady;
            };

            session.SessionReady += handleSessionReady;
            await session.Start();
            Assert.Equal(sessionReadyEvent, SessionReadyEvent.Default);
        }
    }
}
