using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Townsharp.Hosting;

Console.WriteLine("Starting Townsharp Population Monitor!");

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices(ConfigureServices);

await builder.RunConsoleAsync();

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddTownsharp();
    services.AddHostedService<DiscordMarketplace>();
    services.AddLogging(configure =>
    {
        configure.AddConfiguration(context.Configuration.GetSection("Logging"));
    });
}