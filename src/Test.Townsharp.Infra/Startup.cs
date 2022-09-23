using Microsoft.Extensions.DependencyInjection;
using Polly;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Identity;
using Townsharp.Infra.Composition;

namespace Test.Townsharp.Infra
{
    public class Startup
    {
        private IAsyncPolicy<HttpResponseMessage> RetryPolicy = Policy.Handle<OverflowException>().OrResult<HttpResponseMessage>(r => false).RetryAsync();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddClientCredentialsTokenManagement()
                .AddClient(TokenManagementNames.AccountsIssuer, client =>
                {
                    client.TokenEndpoint = AccountsTokenClient.BaseUri;
                    client.ClientId = Environment.GetEnvironmentVariable("TOWNSHARP_TEST_CLIENTID");
                    client.ClientSecret = Environment.GetEnvironmentVariable("TOWNSHARP_TEST_CLIENTSECRET");
                    client.Scope = AccountsTokenClient.DefaultScopes;
                });

            // Consider typed clients with a helper "AddHttpClient" method exposed to library implementers.
            services.AddHttpClient(HttpClientNames.Bot, c => c.BaseAddress = new Uri(ApiClient.BaseAddress))
                .AddPolicyHandler(RetryPolicy)
                .AddClientCredentialsTokenHandler(TokenManagementNames.AccountsIssuer);

            // This is NOT how User Auth works :P
            services.AddHttpClient(HttpClientNames.User, c => c.BaseAddress = new Uri(ApiClient.BaseAddress))
                .AddPolicyHandler(RetryPolicy)
                .AddClientCredentialsTokenHandler(TokenManagementNames.AccountsIssuer);

            services.AddSingleton<ApiClient>();

            HttpClient client = new HttpClient();
        }
    }
}
