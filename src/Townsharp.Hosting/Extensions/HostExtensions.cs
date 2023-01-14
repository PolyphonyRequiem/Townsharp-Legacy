using Microsoft.Extensions.DependencyInjection;
using Polly;
using Townsharp.Api;
using Townsharp.Api.Identity;
using Townsharp.Configuration;
using Townsharp.Infra.Composition;
using Townsharp.Subscriptions;

namespace Townsharp.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTownsharp(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddTownsharp(TownsharpConfig.Default);
        }

        public static IServiceCollection AddTownsharp(this IServiceCollection serviceCollection, TownsharpConfig config)
        {
            IAsyncPolicy<HttpResponseMessage> RetryPolicy = Policy.Handle<OverflowException>().OrResult<HttpResponseMessage>(r => false).RetryAsync();

            serviceCollection.AddSingleton<Session>();           

            serviceCollection.AddSingleton(config);
            serviceCollection.AddDistributedMemoryCache();
            serviceCollection.AddClientCredentialsTokenManagement()
                .AddClient(TokenManagementNames.AccountsIssuer, client =>
                {
                    client.TokenEndpoint = AccountsTokenClient.BaseUri;
                    client.ClientId = Environment.GetEnvironmentVariable("TOWNSHARP_TEST_CLIENTID");
                    client.ClientSecret = Environment.GetEnvironmentVariable("TOWNSHARP_TEST_CLIENTSECRET");
                    client.Scope = AccountsTokenClient.DefaultScopes;
                });

            // Consider typed clients with a helper "AddHttpClient" method exposed to library implementers.
            serviceCollection.AddHttpClient(HttpClientNames.Bot, c => c.BaseAddress = new Uri(Api.ApiClient.BaseAddress))
                .AddPolicyHandler(RetryPolicy)
                .AddClientCredentialsTokenHandler(TokenManagementNames.AccountsIssuer);

            // This is NOT how User Auth works :P
            serviceCollection.AddHttpClient(HttpClientNames.User, c => c.BaseAddress = new Uri(Api.ApiClient.BaseAddress))
                .AddPolicyHandler(RetryPolicy)
                .AddClientCredentialsTokenHandler(TokenManagementNames.AccountsIssuer);

            serviceCollection.AddSingleton<ApiClient>();
            serviceCollection.AddSingleton<AccountsTokenClient>();
            serviceCollection.AddSingleton<SubscriptionClient>();
            serviceCollection.AddSingleton(new AltaClientConfiguration(Environment.GetEnvironmentVariable("TOWNSHARP_TEST_CLIENTID")!));
            return serviceCollection;
        }
    }
}
