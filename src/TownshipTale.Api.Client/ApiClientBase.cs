using System.Text;
using System.Text.Json;

namespace TownshipTale.Api.Client
{
    public abstract class ApiClientBase
    {
        const string apiEndpoint = "https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api/";
        private const string awsApiGatewayKey = "2l6aQGoNes8EHb94qMhqQ5m2iaiOM9666oDTPORf";

        protected ApiClientBase(ClientCredential credential)
            : this(credential, new HttpClient())
        {   
        }

        protected ApiClientBase(ClientCredential credential, HttpClient httpClient)
        {
            this.ClientId = credential.ClientId;
            this.TokenClient = new TokenClient(credential);
            httpClient.BaseAddress = new Uri(apiEndpoint);
            this.HttpClient = httpClient;
        }

        public string ClientId { get; }

        protected TokenClient TokenClient { get; }

        protected HttpClient HttpClient { get;}       

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
            requestMessage.Headers.Add("User-Agent", this.ClientId);
            requestMessage.Headers.Add("Authorization", $"{accessToken.TokenType} {accessToken.Token}");
          
            HttpResponseMessage response = await this.HttpClient.SendAsync(requestMessage, cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                JsonDocument consoleResponseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), options: default, cancellationToken);

                // TODO: This is not the right way to reuse this, or to log for that matter, shame on you if this goes into a Pull Request. - Firoso
                Console.WriteLine(response.Content.ReadAsStringAsync());


                var ipAddress = consoleResponseJson.RootElement.GetProperty("connection").GetProperty("address").GetString()!;
                var websocketPort = consoleResponseJson.RootElement.GetProperty("connection").GetProperty("websocket_port").GetInt32();
                var token = consoleResponseJson.RootElement.GetProperty("token").GetString()!;
                return new ConsoleConnectionInfo(ipAddress, websocketPort, token);
            }
            else return default;
        }
    }
}