using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Townsharp;
using Townsharp.Hosting;

Console.WriteLine("Starting Townsharp WebAPI Subscription Listener!");

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices(ConfigureServices);

await builder.RunConsoleAsync();

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    IAsyncPolicy<HttpResponseMessage> RetryPolicy = Policy.Handle<OverflowException>().OrResult<HttpResponseMessage>(r => false).RetryAsync();
    services.AddTownsharp(new TownsharpConfig());
    services.AddHostedService<SubscriptionListener>();
    services.AddLogging(configure =>
    {
        configure.AddConfiguration(context.Configuration.GetSection("Logging"));
    });
}