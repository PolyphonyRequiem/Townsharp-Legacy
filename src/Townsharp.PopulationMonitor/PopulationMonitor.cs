using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Console;

public class PopulationMonitor : IHostedService
{
    private readonly ConsoleClientFactory consoleClientFactory;
    private readonly ILogger<PopulationMonitor> logger;

    private readonly Dictionary<long, ConsoleClient> KnownConsoles = new Dictionary<long, ConsoleClient>();

    public PopulationMonitor(ConsoleClientFactory clientfactory, ILogger<PopulationMonitor> logger)
    {
        this.consoleClientFactory = clientfactory;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this.MonitorPopulation();
        await Task.Delay(-1);
    }

    private void MonitorPopulation()
    {
        Observable.Interval(TimeSpan.FromSeconds(10))
            .Subscribe(async _ => await CheckPopulations());
    }

    private async Task CheckPopulations()
    {
        var serverId = 103278376;
        // do we have an active console?
        
        if (!KnownConsoles.ContainsKey(serverId))
        {
            // if not, let's get one (and make a note)
            var newConsole = await this.consoleClientFactory.CreateClient(serverId);
            await newConsole.Connect();
            KnownConsoles.Add(serverId, newConsole);
        }
        
        Console.WriteLine($"Console Open for {serverId}");
        var console = KnownConsoles[serverId]!;

        // find a good way to check connectivity.
        //if (!console.IsConnected) // perhaps?
        //{
        //    //recover the console
        //}

        // let's get the population from console
        var response = console.SendCommand("player list");

        Console.WriteLine(response);

        // then let's check it against the results of the web api analysis of the population to see if they are convergent or divergent.

        //ApiClient client = new ApiClient(); // nope, get this shit injected.
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    //var client = await this.clientfactory.CreateClient(103278376);

    //await client.Connect();

    //while (!cancellationToken.IsCancellationRequested)
    //{
    //    var command = Console.ReadLine();

    //    if (command == null)
    //    {
    //        continue;
    //    }

    //    try
    //    {
    //        await client.SendCommand(command);
    //    }
    //    catch (Exception ex)
    //    {
    //        this.logger.LogError("An error occurred handling the command.", ex);
    //    }
    //}
}
