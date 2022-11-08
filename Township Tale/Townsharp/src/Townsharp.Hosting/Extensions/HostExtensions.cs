using Microsoft.Extensions.DependencyInjection;
using Polly;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Configuration;
using Townsharp.Infra.Alta.Identity;
using Townsharp.Infra.Alta.Subscriptions;
using Townsharp.Infra.Composition;

namespace Townsharp.Hosting
{
    public static class ServiceCollectionExtensions
    {
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
            serviceCollection.AddHttpClient(HttpClientNames.Bot, c => c.BaseAddress = new Uri(ApiClient.BaseAddress))
                .AddPolicyHandler(RetryPolicy)
                .AddClientCredentialsTokenHandler(TokenManagementNames.AccountsIssuer);

            // This is NOT how User Auth works :P
            serviceCollection.AddHttpClient(HttpClientNames.User, c => c.BaseAddress = new Uri(ApiClient.BaseAddress))
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
