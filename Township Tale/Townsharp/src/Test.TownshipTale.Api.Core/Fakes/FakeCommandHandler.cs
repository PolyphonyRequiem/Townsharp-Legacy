//namespace Test.TownshipTale.Api.Core.Fakes
//{
//    internal class FakeCommandHandler : ICommandHandler
//    {
//        public bool CanHandle(Command command)
//        {
//            return command is FakeCommand;
//        }

//        public async Task<CommandResult> HandleCommandAsync(Command command, CancellationToken cancellationToken = default)
//        {
//            return await new ValueTask<CommandResult>(new FakeCommandResult());
//        }
//    }
//}
