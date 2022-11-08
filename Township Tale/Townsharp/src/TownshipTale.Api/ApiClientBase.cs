using Polly;
using TownshipTale.Api.Core;

namespace TownshipTale.Api
{
    public abstract class ApiClientBase
    {
        const string apiEndpoint = "https://webapi.townshiptale.com/api/";
        private const string awsApiGatewayKey = "2l6aQGoNes8EHb94qMhqQ5m2iaiOM9666oDTPORf";

        private readonly Func<AccessToken> authorizationCallback;

        private AccessToken accessToken = AccessToken.None;

        private bool AccessTokenIsValid => (accessToken != AccessToken.None && !accessToken.Expiring);

        private static readonly TimeSpan[] sleepDurations = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30) };

        private readonly Policy<AccessToken> AuthorizationPolicy = Policy.HandleResult<AccessToken>(token=>token == AccessToken.None)
                                                                         .WaitAndRetry(sleepDurations);

        private readonly Policy<HttpResponseMessage> ApiPolicy = Policy.HandleResult<HttpResponseMessage>(_ => !_.IsSuccessStatusCode)
                                                                       .WaitAndRetry(sleepDurations);

        protected ApiClientBase(ApiClientConfiguration configuration, Func<AccessToken> authorizationCallback)
            : this(configuration, authorizationCallback, new HttpClient())
        {
            this.authorizationCallback = authorizationCallback;
        }

        protected ApiClientBase(ApiClientConfiguration configuration, Func<AccessToken> authorizationCallback, HttpClient httpClient)
        {
            this.configuration = configuration;
            this.authorizationCallback = authorizationCallback;
            this.HttpClient = httpClient;
            this.HttpClient.BaseAddress = new Uri(apiEndpoint);
        }

        protected ApiClientConfiguration configuration { get; }

        protected HttpClient HttpClient { get; }  

        protected Uri BuildApiEndpointUri(string path)
        {
            UriBuilder uriBuilder = new UriBuilder(apiEndpoint);
            uriBuilder.Path += path;
            return uriBuilder.Uri;
        }

        protected Uri BuildApiEndpointUri(string path, string query)
        {
            UriBuilder uriBuilder = new UriBuilder(apiEndpoint);
            uriBuilder.Path += path;
            uriBuilder.Query = query;
            return uriBuilder.Uri;
        }

        protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
        {
            this.EnsureAccessTokenIsValid(cancellationToken);
            requestMessage.Headers.Add("x-api-key", awsApiGatewayKey);
            requestMessage.Headers.Add("Authorization", $"{this.accessToken}");

            return await this.HttpClient.SendAsync(requestMessage, cancellationToken);
        }

        private void EnsureAccessTokenIsValid(CancellationToken cancellationToken = default)
        {
            // TODO: Consider scoped access tokens.

            if (AccessTokenIsValid)
            {
                return;
            }

            var authorizationResult = this.AuthorizationPolicy.ExecuteAndCapture(this.authorizationCallback);

            if (authorizationResult.Outcome == OutcomeType.Failure)
            {
                if (authorizationResult.FinalException != null)
                {
                    throw new ApiAuthorizationException("Unable to authorization access to the township tale API.", authorizationResult.FinalException);
                }
                else
                {
                    throw new ApiAuthorizationException("Unable to authorization access to the township tale API.");
                }
            }

            this.accessToken = authorizationResult.Result;            
        }

        //public async Task<ConsoleClient> GetConsoleClientAsync (uint consoleId, CancellationToken cancellationToken = default)
        //{
        //    var client = new ConsoleClient(consoleId);
        //    await client.Connect(await GetConsoleInfo(consoleId, cancellationToken), cancellationToken);
        //    return client;
        //}

        //private async Task<ConsoleConnectionInfo> GetConsoleInfo(uint consoleId, CancellationToken cancellationToken = default)
        //{
        //    var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"servers/{consoleId}/console")
        //    {
        //        Content = new StringContent("{\"should_launch\":\"true\",\"ignore_offline\":\"true\"}", Encoding.UTF8, "application/json")
        //    };

        //    var accessToken = await this.TokenClient.GetAuthorizationTokenAsync(cancellationToken);
                        
        //    requestMessage.Headers.Add("x-api-key", awsApiGatewayKey);
        //    requestMessage.Headers.Add("User-Agent", this.ClientId);
        //    requestMessage.Headers.Add("Authorization", $"{accessToken.TokenType} {accessToken.Token}");
          
        //    HttpResponseMessage response = await this.HttpClient.SendAsync(requestMessage, cancellationToken: cancellationToken);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        JsonDocument consoleResponseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), options: default, cancellationToken);

        //        // TODO: This is not the right way to reuse this, or to log for that matter, shame on you if this goes into a Pull Request. - Firoso
        //        Console.WriteLine(response.Content.ReadAsStringAsync());


        //        var ipAddress = consoleResponseJson.RootElement.GetProperty("connection").GetProperty("address").GetString()!;
        //        var websocketPort = consoleResponseJson.RootElement.GetProperty("connection").GetProperty("websocket_port").GetInt32();
        //        var token = consoleResponseJson.RootElement.GetProperty("token").GetString()!;
        //        return new ConsoleConnectionInfo(ipAddress, websocketPort, token);
        //    }
        //    else return default;
        //}
    }
}