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
    public class ApiClient : IServerService, IGroupService
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
        public async Task<GroupInfo> GetGroupDescription(GroupId groupId)
        {
            var groupInfo = (await getBotHttpClient().GetFromJsonAsync<ApiGroup>($"api/groups/{groupId}"))!;
            return MapToGroupInfo(groupInfo);
        }

        public async IAsyncEnumerable<GroupInfo> GetJoinedGroupDescriptions()
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

                foreach (var joinedGroup in await response.Content.ReadFromJsonAsync<ApiJoinedGroup[]>(DefaultSerializerOptions) ?? new ApiJoinedGroup[0])
                {
                    yield return MapToGroupInfo(joinedGroup.Group);
                }
            }
            while (response.Headers.Contains("paginationToken"));            
        }

        public async IAsyncEnumerable<GroupInfo> GetPendingGroupInvitations()
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
                    yield return MapToGroupInfo(invitedGroup);
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

        public async Task<GroupMemberInfo> GetGroupMember(GroupId groupId, UserId userId)
        {
            HttpClient client = getBotHttpClient();
            var response = await client.GetAsync($"api/groups/{groupId}/members/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                throw new InvalidResponseException(errorResponse.ToString());
            }

            var groupMember = await response.Content.ReadFromJsonAsync<ApiGroupMember>(DefaultSerializerOptions);

            return MapToGroupMemberInfo(groupMember!);
        }

        public async IAsyncEnumerable<Townsharp.Servers.ServerInfo> GetJoinedServerDescriptions()
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

                foreach (var joinedServer in await response.Content.ReadFromJsonAsync<ApiServer[]>(DefaultSerializerOptions) ?? new ApiServer[0])
                {
                    yield return MapToServerInfo(joinedServer);
                }
            }
            while (response.Headers.Contains("paginationToken"));
        }

        public async Task<Townsharp.Servers.ServerInfo> GetServerDescription(ServerId serverId)
        {
            HttpClient client = getBotHttpClient();
            var response = await client.GetAsync($"api/servers/{serverId}");

            if (!response.IsSuccessStatusCode)
            {
                ApiErrorResponse errorResponse = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>(DefaultSerializerOptions))!;
                throw new InvalidResponseException(errorResponse.ToString());
            }

            var serverInfo = await response.Content.ReadFromJsonAsync<ApiServer>(DefaultSerializerOptions);

            return MapToServerInfo(serverInfo!);
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
                return new ConsoleAccessResult(IsOnline: false, default, String.Empty);
            }

            var connection = serverJoinResult!.Connection!;

            return new ConsoleAccessResult(IsOnline: true, new Uri($"ws://{connection.Address}:{connection.WebsocketPort}"), serverJoinResult.Token);
        }


        internal static GroupInfo MapToGroupInfo(ApiGroup apiGroup)
        {
            return new GroupInfo(
                new GroupId(apiGroup.Id),
                apiGroup.Name ?? "",
                apiGroup.Description ?? "",
                Enum.Parse<GroupType>(apiGroup.Type));
        }

        internal static GroupMemberInfo MapToGroupMemberInfo(ApiGroupMember apiGroupMember)
        {
            return new GroupMemberInfo(
                new GroupId(apiGroupMember.GroupId),
                new UserId(apiGroupMember.UserId),
                apiGroupMember.Username,
                apiGroupMember.Bot,
                new RoleId(apiGroupMember.RoleId),
                DateTime.Parse(apiGroupMember.CreatedAt),
                Enum.Parse<GroupMemberType>(apiGroupMember.Type));
        }

        internal static ServerInfo MapToServerInfo(ApiServer apiServer)
        {
            return new ServerInfo(
                new ServerId(apiServer.Id),
                new GroupId(apiServer.GroupId),
                apiServer.Name,
                apiServer.Description,
                apiServer.Region);
        }

        // NESTED TYPES
        private delegate HttpClient ClientProvider();
    }
}
