namespace TownshipTale.Api.Core.Server.Console
{
    public interface ICommandHandler
    {
        Task<CommandResult> HandleCommandAsync(Command command, CancellationToken cancellationToken);
    }
}