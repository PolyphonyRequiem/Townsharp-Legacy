using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Users;

namespace Townsharp.Api
{
    public interface IApiClient
    {
        public Task<GroupDescription> GetGroupDescription(GroupId groupId);

        public IAsyncEnumerable<GroupDescription> GetJoinedGroupDescriptions();

        public IAsyncEnumerable<GroupDescription> GetPendingGroupInvitations();

        public Task<bool> AcceptGroupInvite(GroupId groupId);

        public Task<GroupMemberDescription> GetGroupMember(GroupId groupId, UserId userId);

        public IAsyncEnumerable<ServerDescription> GetJoinedServerDescriptions();

        public Task<ServerDescription> GetServerDescription(ServerId serverId);

        public Task<ConsoleAccessResult> RequestConsoleAccess(ServerId serverId);
    }
}
