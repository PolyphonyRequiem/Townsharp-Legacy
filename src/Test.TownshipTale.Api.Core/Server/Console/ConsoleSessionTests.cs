using Test.TownshipTale.Api.Core.Fakes;
using TownshipTale.Api.Core.Server.Console;

namespace Test.TownshipTale.Api.Core.Server.Console
{
    public class ConsoleSessionTests
    {
        [Fact]
        public async Task FakeCommandHandlerRetunsFakeCommandResult()
        {
            var consoleSession = new ConsoleSession(new FakeCommandHandler());
            var result = await consoleSession.ExecuteCommandAsync(new FakeCommand());
            Assert.True(result is FakeCommandResult);
        }
    }
}
