using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Townsharp;
using Townsharp.Hosting;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Configuration;
using Townsharp.Infra.Alta.Identity;
using Townsharp.Infra.Alta.Subscriptions;
using Townsharp.Infra.Composition;

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
        configure.AddFile(config =>
        {
            config.RootPath = AppContext.BaseDirectory;
        });
    });
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
    services.AddSingleton<AccountsTokenClient>();
    services.AddSingleton<SubscriptionClient>();
    services.AddSingleton(new AltaClientConfiguration(Environment.GetEnvironmentVariable("TOWNSHARP_TEST_CLIENTID")!));

    HttpClient client = new HttpClient();
}