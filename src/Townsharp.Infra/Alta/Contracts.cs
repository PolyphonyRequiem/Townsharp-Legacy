using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Users;

namespace Townsharp.Infra.Alta
{
    internal record class ConsoleConnectionInfo(long ServerId, string Address, string LocalAddress, string PodName, int GamePort, int ConsolePort, int LoggingPort, int WebsocketPort, int WebserverPort);

    internal record GroupInfo(long Id, string? Name, string? Description, int MemberCount, string CreatedAt, string Type, string[] Tags, GroupServerInfo[] Servers, int? AllowedServerCount, GroupRoleInfo[] Roles)
    {
        internal GroupDescription MapToGroupDescriptor()
        {
            return new GroupDescription(
                new GroupId(this.Id),
                this.Name ?? "",
                this.Description ?? "",
                Enum.Parse<GroupType>(this.Type));
        }
    };

    internal record GroupMemberInfo(long GroupId, long UserId, string Username, bool Bot, long Icon, string Permissions, long RoleId, string CreatedAt, string Type)
    {
        internal GroupMemberDescription MapToMemberDescriptor()
        {
            return new GroupMemberDescription(
                new GroupId(GroupId),
                new UserId(UserId),
                Username,
                Bot,
                new RoleId(RoleId),
                DateTime.Parse(CreatedAt),
                Enum.Parse<GroupMemberType>(this.Type));
        }
    }

    internal record GroupRoleInfo(int RoleId, string Name, string? Color, string[] Permissions, string[] AllowedCommands);

    internal record GroupServerInfo(long Id, string Name, int SceneIndex, string Status);

    internal record InvitedGroupInfo(
        long Id,
        string? Name,
        string? Description,
        int MemberCount,
        string CreatedAt,
        string Type,
        string[] Tags,
        GroupServerInfo[] Servers,
        int? AllowedServerCount,
        GroupRoleInfo[] Roles,
        string InvitedAt) : GroupInfo(Id, Name, Description, MemberCount, CreatedAt, Type, Tags, Servers, AllowedServerCount, Roles);

    internal record JoinedGroupInfo(
        GroupInfo Group,
        GroupMemberInfo Member);

    internal record ServerJoinResult(long ServerId, bool Allowed, bool WasRejection, bool ColdStart, string FailReason, string? Message, ConsoleConnectionInfo? Connection, string Token);

    internal record ServerInfo(
        long Id,
        string Name,
        ServerOnlinePlayerInfo[] OnlinePlayers,
        string ServerStatus,
        string FinalStatus,
        int SceneIndex,
        int Target,
        string Region,
        DateTime? OnlinePing,
        DateTime LastOnline,
        string Description,
        float Playability,
        string Version,
        long GroupId,
        string OwnerType,
        long OwnerId,
        string Type,
        string JoinType,
        string ServerFleet,
        TimeSpan UpTime,
        bool IsOnline)
    {
        internal ServerDescription MapToServerDescriptor()
        {
            return new ServerDescription(
                new ServerId(Id),
                new GroupId(GroupId),
                Name,
                Description,
                Region);
        }
    }

    internal record ServerOnlinePlayerInfo(long Id, string Username);

    internal record ApiErrorResponse(string Message, string ErrorCode);
}
