using System.Collections.Concurrent;

namespace TownshipTale.Api.Core.Server.Console
{
    public sealed class ConsoleSession
    {
        public ConsoleSession(ICommandHandler commandHandler)
        {
            this.Handler = commandHandler;
        }


        public ICommandHandler Handler { get; }
        
        public async Task<CommandResult> ExecuteCommandAsync(Command command, CancellationToken cancellationToken = default)
        {
            return await this.Handler.HandleCommandAsync(command, cancellationToken);
        }
        //system.threading.channels?
    }
}