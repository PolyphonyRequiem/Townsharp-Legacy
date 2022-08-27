using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Townsharp;
using Townsharp.Hosting;

Console.WriteLine("Starting Townsharp WebAPI Subscription Listener!");

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddTownsharp(new TownsharpConfig(new Townsharp.Identity.IdentityConfig()));
        services.AddHostedService<SubscriptionListener>();
    });

await builder.RunConsoleAsync();