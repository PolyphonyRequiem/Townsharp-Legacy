using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace TownshipTale.Api.Core
{
    public class TokenClient
    {
        const string tokenEndpoint = "https://accounts.townshiptale.com/connect/token";
        const string scopes = "ws.group ws.group_members ws.group_servers ws.group_bans ws.group_invites group.info group.join group.leave group.view group.members group.invite server.view server.console";
        private readonly ClientCredential credential;
        private readonly ILogger logger;
        private readonly TimeSpan refreshBy = TimeSpan.FromMinutes(5);

        private bool tokenIsStale => DateTime.Now >= tokenExpiresAt - refreshBy;

        private DateTime tokenExpiresAt => lastRefreshed + TimeSpan.FromSeconds(cachedAccessToken.ExpiresIn);

        private DateTime lastRefreshed = DateTime.MinValue;
        private AccessToken cachedAccessToken = default;

        public TokenClient(ClientCredential credential, ILogger logger)
        {
            HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(tokenEndpoint)
            };
            this.credential = credential;
            this.logger = logger;
        }

        protected HttpClient HttpClient { get; }

        public async Task<AccessToken> GetAuthorizationTokenAsync(CancellationToken cancellationToken = default)
        {
            if (cachedAccessToken != default && !tokenIsStale)
            {
                return cachedAccessToken;
            }

            var payload = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials" },
                {"scope", scopes },
                {"client_id", credential.ClientId },
                {"client_secret", credential.ClientSecret }
            };
            var accessToken = await Policy.Handle<Exception>()
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, Math.Max(retryAttempt, 10))),
                (exception, timespan) =>
                {
                    this.logger.LogWarning("Retrying request for authorization token.");
                })
                .ExecuteAndCaptureAsync(async () =>
                {
                    HttpResponseMessage response = await HttpClient.PostAsync(requestUri: string.Empty, content: new FormUrlEncodedContent(payload), cancellationToken: cancellationToken);
                    JsonDocument tokenResponse = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), options: default, cancellationToken);

                    if (tokenResponse.RootElement.EnumerateObject().Any(element => element.Name == "error"))
                    {
                        throw new HttpRequestException(tokenResponse.RootElement.GetProperty("error").GetString(), inner: default, response.StatusCode);
                    }
                    else
                    {
                        var token = new AccessToken
                        {
                            Token = tokenResponse.RootElement.GetProperty("access_token").GetString()!,
                            ExpiresIn = tokenResponse.RootElement.GetProperty("expires_in").GetInt32(),
                            Scope = tokenResponse.RootElement.GetProperty("scope").GetString()!,
                            TokenType = tokenResponse.RootElement.GetProperty("token_type").GetString()!,
                        };

                        lastRefreshed = DateTime.Now;
                        cachedAccessToken = token;
                        return token;
                    }
                });            
        }
    }
}
