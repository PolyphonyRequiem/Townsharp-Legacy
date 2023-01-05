using Townsharp.Users;

namespace Townsharp.Groups
{
    public interface IGroupService
    {
        Task<GroupInfo> GetGroupDescription(GroupId groupId);

        IAsyncEnumerable<GroupInfo> GetJoinedGroupDescriptions();

        IAsyncEnumerable<GroupInfo> GetPendingGroupInvitations();

        Task<bool> AcceptGroupInvite(GroupId groupId);

        Task<GroupMemberInfo> GetGroupMember(GroupId groupId, UserId userId);
    }
}
