using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Townsharp.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTownsharp(this IServiceCollection serviceCollection, TownsharpConfig config)
        {
            serviceCollection.AddMediatR(typeof(Session));
            serviceCollection.AddSingleton<Session>();
            serviceCollection.AddSingleton(config);
            return serviceCollection;
        }
    }
}
