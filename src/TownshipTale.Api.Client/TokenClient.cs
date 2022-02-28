using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TownshipTale.Api.Client
{
    public class TokenClient
    {
        const string tokenEndpoint = "https://accounts.townshiptale.com/connect/token";
        const string scopes = "ws.group ws.group_members ws.group_servers ws.group_bans ws.group_invites group.info group.join group.leave group.view group.members group.invite server.view server.console";
        private readonly ClientCredential credential;
        private readonly TimeSpan refreshBy = TimeSpan.FromMinutes(5);
        
        private bool tokenIsStale => DateTime.Now >= tokenExpiresAt - refreshBy;

        private DateTime tokenExpiresAt => lastRefreshed + TimeSpan.FromSeconds(cachedAccessToken.ExpiresIn);

        private DateTime lastRefreshed = DateTime.MinValue;
        private AccessToken cachedAccessToken = default;
                
        public TokenClient(ClientCredential credential)
        {
            this.HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(tokenEndpoint)
            };
            this.credential = credential;
        }

        protected HttpClient HttpClient { get; }

        public async Task<AccessToken> GetAuthorizationTokenAsync(CancellationToken cancellationToken)
        {
            if (this.cachedAccessToken != default && !this.tokenIsStale)
            {
                return this.cachedAccessToken;
            }

            var payload = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials" },
                {"scope", scopes },
                {"client_id", this.credential.ClientId },
                {"client_secret", this.credential.ClientSecret }
            };

            HttpResponseMessage response = await this.HttpClient.PostAsync(requestUri: String.Empty, content: new FormUrlEncodedContent(payload), cancellationToken: cancellationToken);
            JsonDocument tokenResponse = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), options: default, cancellationToken);

            if (tokenResponse.RootElement.EnumerateObject().Any(element=>element.Name == "error"))
            {
                throw new HttpRequestException(tokenResponse.RootElement.GetProperty("error").GetString(),  inner: default, response.StatusCode);
            }
            else
            {
                var token = new AccessToken
                {
                    Token = tokenResponse.RootElement.GetProperty("access_token").GetString()!,
                    ExpiresIn = tokenResponse.RootElement.GetProperty("expires_in").GetInt32(),
                    Scope = tokenResponse.RootElement.GetProperty("scope").GetString()?.Split(' ')!,
                    TokenType = tokenResponse.RootElement.GetProperty("token_type").GetString()!,
                };

                this.lastRefreshed = DateTime.Now;
                this.cachedAccessToken = token;
                return token;
            }
        }        
    }
}
