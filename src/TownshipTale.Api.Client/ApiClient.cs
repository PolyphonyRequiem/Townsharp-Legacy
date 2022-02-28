using System.Text;
using System.Text.Json;

namespace TownshipTale.Api.Client
{
    public class ApiClient
    {
        const string apiEndpoint = "https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/";
        private const string awsApiGatewayKey = "2l6aQGoNes8EHb94qMhqQ5m2iaiOM9666oDTPORf";
        private readonly string clientId;

        protected ApiClient(ClientCredential credential)
        {
            this.clientId =credential.ClientId;
            this.TokenClient = new TokenClient(credential);
            this.HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(apiEndpoint)
            };
        }

        protected TokenClient TokenClient { get; }
        protected HttpClient HttpClient { get;}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Ideal for containerized environments.</remarks>
        public static Task<ApiClient> CreateAuthenticatedClientAsync(CancellationToken cancellationToken = default)
        {
            return CreateAuthenticatedClientAsync(ClientCredential.FromEnvironment(), cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        /// <remarks>Intended for console clients.</remarks>
        public static Task<ApiClient> CreateAuthenticatedClientAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default)
        {
            return CreateAuthenticatedClientAsync(new ClientCredential(clientId, clientSecret), cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        /// <remarks>Intended to be used by dependency injection and factory scenarios.</remarks>
        public static async Task<ApiClient> CreateAuthenticatedClientAsync(ClientCredential credentials, CancellationToken cancellationToken = default)
        {
            var client = new ApiClient(credentials);

            await client.TokenClient.GetAuthorizationTokenAsync(cancellationToken);

            return client;
        }        

        public async Task<ConsoleClient> GetConsoleClientAsync (uint consoleId, CancellationToken cancellationToken = default)
        {
            var client = new ConsoleClient(consoleId);
            await client.Connect(await GetConsoleInfo(consoleId, cancellationToken), cancellationToken);
            return client;
        }

        private async Task<ConsoleConnectionInfo> GetConsoleInfo(uint consoleId, CancellationToken cancellationToken = default)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"servers/{consoleId}/console")
            {
                Content = new StringContent("{\"should_launch\":\"true\",\"ignore_offline\":\"true\"}", Encoding.UTF8, "application/json")
            };

            var accessToken = await this.TokenClient.GetAuthorizationTokenAsync(cancellationToken);
                        
            requestMessage.Headers.Add("x-api-key", awsApiGatewayKey);
            requestMessage.Headers.Add("User-Agent", this.clientId);
            requestMessage.Headers.Add("Authorization", $"{accessToken.TokenType} {accessToken.Token}");
          
            HttpResponseMessage response = await this.HttpClient.SendAsync(requestMessage, cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                JsonDocument consoleResponseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), options: default, cancellationToken);

                var ipAddress = consoleResponseJson.RootElement.GetProperty("connection").GetProperty("address").GetString()!;
                var websocketPort = consoleResponseJson.RootElement.GetProperty("connection").GetProperty("websocket_port").GetInt32();
                var token = consoleResponseJson.RootElement.GetProperty("token").GetString()!;
                return new ConsoleConnectionInfo(ipAddress, websocketPort, token);
            }
            else return default;
        }
    }
}