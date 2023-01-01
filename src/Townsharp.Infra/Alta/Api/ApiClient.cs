using Microsoft.Extensions.Logging;
using static Townsharp.Infra.Alta.Json.JsonUtils;
using Townsharp.Infra.Composition;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Users;
using System.Net.Http.Json;
using Townsharp.Consoles;

namespace Townsharp.Infra.Alta.Api
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
            this.getBotHttpClient = () => httpClientFactory.CreateClient(HttpClientNames.Bot);
            this.getUserHttpClient = () => httpClientFactory.CreateClient(HttpClientNames.User);
        }

        // Allow client type to be specified where possible, User, Bot, Auto.
        public async Task<GroupDescription> GetGroupDescription(GroupId groupId)
        {
            var groupInfo = (await getBotHttpClient().GetFromJsonAsync<GroupInfo>($"api/groups/{groupId}"))!;
            return groupInfo.MapToGroupDescriptor();
        }

        public async IAsyncEnumerable<GroupDescription> GetJoinedGroupDescriptions()
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
                    String.Empty;

                foreach (var joinedGroup in await response.Content.ReadFromJsonAsync<JoinedGroupInfo[]>(DefaultSerializerOptions) ?? new JoinedGroupInfo[0])
                {
                    yield return joinedGroup.Group.MapToGroupDescriptor();
                }
            }
            while (response.Headers.Contains("paginationToken"));            
        }

        public async IAsyncEnumerable<GroupDescription> GetPendingGroupInvitations()
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

                foreach (var invitedGroup in await response.Content.ReadFromJsonAsync<InvitedGroupInfo[]>(DefaultSerializerOptions) ?? new InvitedGroupInfo[0])
                {
                    yield return invitedGroup.MapToGroupDescriptor();
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

        public async Task<GroupMemberDescription> GetGroupMember(GroupId groupId, UserId userId)
        {
            HttpClient client = getBotHttpClient();
            var response = await client.GetAsync($"api/groups/{groupId}/members/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                throw new InvalidResponseException(errorResponse.ToString());
            }

            var groupMember = await response.Content.ReadFromJsonAsync<GroupMemberInfo>(DefaultSerializerOptions);

            return groupMember!.MapToMemberDescriptor();
        }

        public async IAsyncEnumerable<ServerDescription> GetJoinedServerDescriptions()
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
                    String.Empty;

                foreach (var joinedServer in await response.Content.ReadFromJsonAsync<ServerInfo[]>(DefaultSerializerOptions) ?? new ServerInfo[0])
                {
                    yield return joinedServer.MapToServerDescriptor();
                }
            }
            while (response.Headers.Contains("paginationToken"));
        }

        public async Task<ServerDescription> GetServerDescription(ServerId serverId)
        {
            HttpClient client = getBotHttpClient();
            var response = await client.GetAsync($"api/servers/{serverId}");

            if (!response.IsSuccessStatusCode)
            {
                ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                throw new InvalidResponseException(errorResponse.ToString());
            }

            var serverInfo = await response.Content.ReadFromJsonAsync<ServerInfo>(DefaultSerializerOptions);

            return serverInfo!.MapToServerDescriptor();
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

            var serverJoinResult = await response.Content.ReadFromJsonAsync<ServerJoinResult>(DefaultSerializerOptions);

            if (!serverJoinResult?.Allowed ?? false)
            {
                return new ConsoleAccessResult(IsOnline: false, default, String.Empty);
            }

            var connection = serverJoinResult!.Connection!;

            return new ConsoleAccessResult(IsOnline: true, new Uri($"ws://{connection.Address}:{connection.WebsocketPort}"), serverJoinResult.Token);
        }

        // NESTED TYPES

        private delegate HttpClient ClientProvider();
    }
}
