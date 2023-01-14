using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Townsharp;
using Townsharp.Hosting;

Console.WriteLine("Starting Townsharp SubscriptionClientTests!");

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices(ConfigureServices);

await builder.RunConsoleAsync();

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddTownsharp(new TownsharpConfig());
    services.AddHostedService<SubscriptionClientTest>();
    services.AddLogging(configure =>
    {
        configure.AddConfiguration(context.Configuration.GetSection("Logging"));
    });
}