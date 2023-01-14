using Microsoft.Extensions.Logging;
using static Townsharp.Api.Json.JsonUtils;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Users;
using System.Net.Http.Json;
using Townsharp.Consoles;
using Townsharp.Api.Composition;

namespace Townsharp.Api
{
    public class ApiClient
    {
        public const int Limit = 100;
        public const string BaseAddress = "https://webapi.townshiptale.com/";

        private readonly ILogger<ApiClient> logger;
        private readonly ClientProvider getBotHttpClient;
        private readonly ClientProvider getUserHttpClient;

        public ApiClient(IHttpClientFactory httpClientFactory, ILogger<ApiClient> logger)
        {
            this.logger = logger;
            getBotHttpClient = () => httpClientFactory.CreateClient(HttpClientNames.Bot);
            getUserHttpClient = () => httpClientFactory.CreateClient(HttpClientNames.User);
        }

        // Allow client type to be specified where possible, User, Bot, Auto.
        public async Task<ApiGroup> GetGroup(GroupId groupId)
        {
            return (await getBotHttpClient().GetFromJsonAsync<ApiGroup>($"api/groups/{groupId}", DefaultSerializerOptions))!;            
        }

        public async IAsyncEnumerable<ApiJoinedGroup> GetJoinedGroups()
        {
            HttpClient client = getBotHttpClient();
            HttpResponseMessage response;
            string lastPaginationToken = string.Empty;

            do
            {
                var message = lastPaginationToken != string.Empty ?
                    new HttpRequestMessage(HttpMethod.Get, $"api/groups/joined?limit={Limit}&paginationToken={lastPaginationToken}") :
                    new HttpRequestMessage(HttpMethod.Get, $"api/groups/joined?limit={Limit}");

                response = await client.SendAsync(message);

                if (!response.IsSuccessStatusCode)
                {
                    ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                    throw new InvalidResponseException(errorResponse.ToString());
                }

                lastPaginationToken = response.Headers.Contains("paginationToken") ?
                    response.Headers.GetValues("paginationToken").First() :
                    string.Empty;

                foreach (var joinedGroup in await response.Content.ReadFromJsonAsync<ApiJoinedGroup[]>(DefaultSerializerOptions) ?? new ApiJoinedGroup[0])
                {
                    yield return joinedGroup;
                }
            }
            while (response.Headers.Contains("paginationToken"));
        }

        public async IAsyncEnumerable<ApiInvitedGroup> GetPendingGroupInvitations()
        {
            HttpClient client = getBotHttpClient();
            HttpResponseMessage response;
            string lastPaginationToken = string.Empty;

            do
            {
                var message = lastPaginationToken != string.Empty ?
                    new HttpRequestMessage(HttpMethod.Get, $"api/groups/invites?limit={Limit}&paginationToken={lastPaginationToken}") :
                    new HttpRequestMessage(HttpMethod.Get, $"api/groups/invites?limit={Limit}");

                response = await client.SendAsync(message);

                if (!response.IsSuccessStatusCode)
                {
                    ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                    throw new InvalidResponseException(errorResponse.ToString());
                }

                if (response.Headers.Contains("paginationToken"))
                {
                    lastPaginationToken = response.Headers.GetValues("paginationToken").First();
                }

                foreach (var invitedGroup in await response.Content.ReadFromJsonAsync<ApiInvitedGroup[]>(DefaultSerializerOptions) ?? new ApiInvitedGroup[0])
                {
                    yield return invitedGroup;
                }
            }
            while (response.Headers.Contains("paginationToken"));
        }

        // throws 400 if the invite has already been accepted
        public async Task<bool> AcceptGroupInvite(GroupId groupId)
        {
            HttpClient client = getBotHttpClient();
            var response = await client.PostAsync($"api/groups/invites/{groupId}", new StringContent(groupId.ToString()));
            return response.IsSuccessStatusCode;
        }

        public async Task<ApiGroupMember> GetGroupMember(GroupId groupId, UserId userId)
        {
            HttpClient client = getBotHttpClient();
            var response = await client.GetAsync($"api/groups/{groupId}/members/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                throw new InvalidResponseException(errorResponse.ToString());
            }

            var groupMember = await response.Content.ReadFromJsonAsync<ApiGroupMember>(DefaultSerializerOptions);

            return groupMember!;
        }

        public async IAsyncEnumerable<ApiServer> GetJoinedServersAsync()
        {
            HttpClient client = getBotHttpClient();
            HttpResponseMessage response;
            string lastPaginationToken = string.Empty;

            do
            {
                var message = lastPaginationToken != string.Empty ?
                    new HttpRequestMessage(HttpMethod.Get, $"api/servers/joined?limit={Limit}&paginationToken={lastPaginationToken}") :
                    new HttpRequestMessage(HttpMethod.Get, $"api/servers/joined?limit={Limit}");

                response = await client.SendAsync(message);

                if (!response.IsSuccessStatusCode)
                {
                    ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                    throw new InvalidResponseException(errorResponse.ToString());
                }

                lastPaginationToken = response.Headers.Contains("paginationToken") ?
                    response.Headers.GetValues("paginationToken").First() :
                    string.Empty;

                foreach (var joinedServer in await response.Content.ReadFromJsonAsync<ApiServer[]>(DefaultSerializerOptions) ?? new ApiServer[0])
                {
                    yield return joinedServer;
                }
            }
            while (response.Headers.Contains("paginationToken"));
        }

        public async Task<ApiServer> GetServerAsync(ServerId serverId)
        {
            HttpClient client = getBotHttpClient();
            var response = await client.GetAsync($"api/servers/{serverId}");

            if (!response.IsSuccessStatusCode)
            {
                ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                throw new InvalidResponseException(errorResponse.ToString());
            }

            var serverInfo = await response.Content.ReadFromJsonAsync<ApiServer>(DefaultSerializerOptions);

            return serverInfo!;
        }

        public async Task<ConsoleAccessResult> RequestConsoleAccess(ServerId serverId)
        {
            HttpClient client = getBotHttpClient();
            var response = await client.PostAsync($"api/servers/{serverId}", new StringContent("{\"should_launch\":true, \"ignore_offline\":true}"));

            if (!response.IsSuccessStatusCode)
            {
                ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                throw new InvalidResponseException(errorResponse.ToString());
            }

            var serverJoinResult = await response.Content.ReadFromJsonAsync<ApiConsoleResponse>(DefaultSerializerOptions);

            if (!serverJoinResult?.Allowed ?? false)
            {
                return new ConsoleAccessResult(IsOnline: false, default, string.Empty);
            }

            var connection = serverJoinResult!.Connection!;

            return new ConsoleAccessResult(IsOnline: true, new Uri($"ws://{connection.Address}:{connection.WebsocketPort}"), serverJoinResult.Token);
        }

        // NESTED TYPES
        private delegate HttpClient ClientProvider();
    }
}
