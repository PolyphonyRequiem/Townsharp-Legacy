using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Townsharp.Infra.Alta.Console;

public class ConsoleRepl : IHostedService
{
    private readonly ConsoleClientFactory clientfactory;
    private readonly ILogger<ConsoleRepl> logger;

    public ConsoleRepl(ConsoleClientFactory clientfactory, ILogger<ConsoleRepl> logger)
    {
        this.clientfactory = clientfactory;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = this.Repl(cancellationToken);
        await Task.Delay(-1);
    }

    private async Task Repl(CancellationToken cancellationToken)
    {
        var client = await this.clientfactory.CreateClient(103278376); 
        //var client = await this.clientfactory.CreateClient(1174503463); // Cairnbrook
        await client.Connect();

        while (!cancellationToken.IsCancellationRequested)
        {
            var command = Console.ReadLine();

            if (command == null)
            {
                continue;
            }

            try
            {
                await client.SendCommand(command);
            }
            catch (Exception ex) 
            { 
                this.logger.LogError("An error occurred handling the command.", ex);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}