using TownshipTale.Api.Core.Server.Console;

namespace Test.TownshipTale.Api.Core.Fakes
{
    internal record FakeCommandResult : CommandResult
    {
        public override string ResultContent => "FakeResult";
    }
}
