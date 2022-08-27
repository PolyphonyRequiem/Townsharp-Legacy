namespace Townsharp.WebApi
{
    // THIS VERY MUCH FEELS LIKE INFRA, NOT CORE MODEL!

    public class ApiClient
    {
        public ApiClient()
        {

        }

        public Task<GroupMemberInfo> AcceptGroupInviteAsync(long groupId)
        {
            throw new NotImplementedException();
        }

        public Task<GroupInfo> GetGroupInfoAsync(long groupId)
        {
            throw new NotImplementedException();
        }

        public Task<GroupMemberInfo> GetGroupMemberAsync(long groupId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<JoinedGroupInfo[]> GetJoinedGroupsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<InvitedGroupInfo> GetPendingGroupInvitesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServerConnectionInfo> GetServerConnectionInfo(long serverId)
        {
            throw new NotImplementedException();
        }

        public Task<ServerInfo> GetServerInfo(long serverId)
        {
            throw new NotImplementedException();
        }
    }
}
