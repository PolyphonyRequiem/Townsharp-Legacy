using Townsharp.Consoles;
using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Users;

namespace Townsharp.Api
{
    public interface IApiClient
    {
        public Task<GroupDescription> GetGroup(GroupId groupId);

        public IAsyncEnumerable<GroupDescription> GetJoinedGroups();

        public IAsyncEnumerable<GroupDescription> GetPendingGroupInvitations();

        public Task<bool> AcceptGroupInvite(GroupId groupId);

        public Task<GroupMemberDescription> GetGroupMember(GroupId groupId, UserId userId);

        public Task<ServerDescription> GetServerDescriptor(ServerId serverId);

        public Task<ConsoleAccessResult> RequestConsoleAccess(ServerId serverId);
    }
}
