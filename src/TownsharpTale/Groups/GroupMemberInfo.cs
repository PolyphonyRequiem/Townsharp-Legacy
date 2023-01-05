using Townsharp.Users;

namespace Townsharp.Groups
{
    // not sure this belongs here? same with IDs
    public record GroupMemberInfo(GroupId GroupId, UserId UserId, string Username, bool IsBot, RoleId RoleId, DateTime CreatedAt, GroupMemberType MemberType);

    public enum GroupMemberType
    {
        Invited,
        Accepted,
        Requested,
        Banned
    }
}
