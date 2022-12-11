using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Users;

namespace Townsharp.Api
{
    public interface IApiClient
    {
        public Task<GroupMemberInfo> AcceptGroupInvite(GroupId groupId);

        public Task<GroupInfo> GetGroupInfo(GroupId groupId);

        public Task<GroupMemberInfo> GetGroupMember(GroupId groupId, UserId userId);

        public Task<JoinedGroupInfo[]> GetJoinedGroups();

        public Task<InvitedGroupInfo[]> GetPendingGroupInvites();

        public Task<ConsoleSessionInfo> GetConsoleInfo(ServerId serverId);

        public Task<ServerInfo> GetServerInfo(ServerId serverId);
    }
}
