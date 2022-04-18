namespace TownshipTale.Api.Server.Console
{
    public interface IConsoleClient
    {
        Task<CommandResult> ExecuteCommandAsync(Command command, CancellationToken cancellationToken = default);
    }
}