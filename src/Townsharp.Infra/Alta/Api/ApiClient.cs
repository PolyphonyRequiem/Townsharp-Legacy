using Microsoft.Extensions.Logging;
using System.Runtime.Serialization;
using System.Text.Json;
using Townsharp.Infra.Composition;

namespace Townsharp.Infra.Alta.Api
{
    public class ApiClient
    {
        public const string BaseAddress = "https://webapi.townshiptale.com/"; 
        
        private readonly ILogger<ApiClient> logger;
        private readonly JsonSerializerOptions serializerOptions;
        private readonly ClientProvider botClientProvider;
        private readonly ClientProvider userClientProvider;
        
        public ApiClient(IHttpClientFactory httpClientFactory, ILogger<ApiClient> logger)
        {
            this.logger = logger;
            this.serializerOptions = WebApiJsonSerializerOptions.Default;
            this.botClientProvider = () => httpClientFactory.CreateClient(HttpClientNames.Bot);
            this.userClientProvider = () => httpClientFactory.CreateClient(HttpClientNames.User);
        }

        // throws 400 if the invite has already been accepted
        public Task<GroupMemberInfo> AcceptGroupInvite(long groupId) => PostAsBot<GroupMemberInfo>($"api/groups/invites/{groupId}", $"{groupId}");

        public Task<GroupInfo> GetGroupInfo(long groupId) => GetAsBot<GroupInfo>($"api/groups/{groupId}");

        public Task<GroupMemberInfo> GetGroupMember(long groupId, long userId) => GetAsBot<GroupMemberInfo>($"api/groups/{groupId}/members/{userId}");

        public Task<JoinedGroupInfo[]> GetJoinedGroups() => GetAsBot<JoinedGroupInfo[]>($"api/groups/joined?limit=1000");

        public Task<InvitedGroupInfo[]> GetPendingGroupInvites() => GetAsBot<InvitedGroupInfo[]>($"api/groups/invites?limit=1000");

        public Task<ServerConnectionInfo> GetServerConnectionInfo(long serverId) => PostAsBot<ServerConnectionInfo>($"api/servers/{serverId}/console", "{\"should_launch\":true, \"ignore_offline\":true}");

        public Task<ServerInfo> GetServerInfo(long serverId) => GetAsBot<ServerInfo>($"api/servers/{serverId}");

        private Task<T> GetAsUser<T>(string path) => Get<T>(this.userClientProvider, path);

        private Task<T> GetAsBot<T>(string path) => Get<T>(this.botClientProvider, path);

        private async Task<T> Get<T>(ClientProvider clientProvider, string path)
        {
            logger.LogDebug($"Requesting {path}");
            var response = await clientProvider.Invoke().GetAsync(path);
            return await EnsureSuccessAndSerializeResponse<T>(response);
        }

        private Task<T> PostAsUser<T>(string path, string body = "") => Post<T>(this.userClientProvider, path, body);

        private Task<T> PostAsBot<T>(string path, string body = "") => Post<T>(this.botClientProvider, path, body);

        private async Task<T> Post<T>(ClientProvider clientProvider, string path, string body = "")
        {
            logger.LogDebug($"Requesting {path}");
            var response = await clientProvider.Invoke().PostAsync(path, new StringContent(body));
            return await EnsureSuccessAndSerializeResponse<T>(response);
        }

        private async Task<T> EnsureSuccessAndSerializeResponse<T>(HttpResponseMessage response)
        {
            logger.LogDebug($"Response: {Environment.NewLine}{response}");
            response.EnsureSuccessStatusCode();
            T? result = await JsonSerializer.DeserializeAsync<T>(response.Content.ReadAsStream(), serializerOptions);
            return result == null ? throw new InvalidResponseException() : result;
        }

        private delegate HttpClient ClientProvider();
    }
}
