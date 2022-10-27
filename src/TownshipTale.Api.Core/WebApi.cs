using TownshipTale.Api.Core.Api.Schemas;

namespace TownshipTale.Api.Core
{
    internal class WebApi
    {
        private ApiClient client;

        public WebApi(ApiClient client)
        {
            this.client = client;
        }

        public void Authorize()
        {
            throw new NotImplementedException();
        }

        public async Task AcceptGroupInviteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public async Task<GroupInfo> GetGroupInfoAsync(long groupId)
        {
            throw new NotImplementedException();
        }

        public async Task<GroupMemberInfo> GetGroupMemberAsync(long groupId, string userid)
        {
            throw new NotImplementedException();
        }

        public async Task<JoinedGroupInfo[]> GetJoinedGroupsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
