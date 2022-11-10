using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Townsharp;
using Townsharp.Hosting;

Console.WriteLine("Starting Townsharp Population Monitor!");

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices(ConfigureServices);

await builder.RunConsoleAsync();

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddTownsharp(new TownsharpConfig());
    services.AddHostedService<PopulationMonitor>();
    services.AddLogging(configure =>
    {
        configure.AddConfiguration(context.Configuration.GetSection("Logging"));
    });

    services.AddSingleton<ConsoleManager>();
    services.AddSingleton<ConsoleClientFactory>();
}