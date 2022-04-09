using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TownshipTale.Api.Core.Server.Console;
using TownshipTale.Api.Identity;
using TownshipTale.Api.Server.Console;

namespace TownshipTale.Api
{
    public class WebApiClient : ApiClientBase
    {
        public WebApiClient(ApiClientConfiguration configuration, Func<AccessToken> authorizationCallback) 
            : base(configuration, authorizationCallback)
        {
        }

        public async Task<ConsoleConnectionInfo> GetConsoleConnectionInfoAsync(int serverId, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, base.BuildApiEndpointUri($"servers/{serverId}/console"));

            request.Content = new StringContent("{\"should_launch\":true,\"ignore_offline\":true}");

            var response = await base.SendRequestAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // TODO: Other domain exceptions.
                throw new ApiClientException("Unable to get Console Connection Info at this time.  Server is probably not connected.");
            }

            JsonDocument consoleResponseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), options: default, cancellationToken);

            // TODO: This is not the right way to reuse this, or to log for that matter, shame on you if this goes into a Pull Request
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            var ipAddress = consoleResponseJson.RootElement.GetProperty("connection").GetProperty("address").GetString()!;
            var websocketPort = consoleResponseJson.RootElement.GetProperty("connection").GetProperty("websocket_port").GetInt32();
            var token = consoleResponseJson.RootElement.GetProperty("token").GetString()!;
            return new ConsoleConnectionInfo(ipAddress, websocketPort, token, serverId);
        }
    }
}
