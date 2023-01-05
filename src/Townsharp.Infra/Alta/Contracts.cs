using Townsharp.Groups;
using Townsharp.Servers;
using Townsharp.Users;

namespace Townsharp.Infra.Alta
{
    internal record class ApiConsoleConnection(long ServerId, string Address, string LocalAddress, string PodName, int GamePort, int ConsolePort, int LoggingPort, int WebsocketPort, int WebserverPort);

    internal record ApiGroup(long Id, string? Name, string? Description, int MemberCount, string CreatedAt, string Type, string[] Tags, ApiGroupServer[] Servers, int? AllowedServerCount, ApiGroupRole[] Roles)
    {

    };

    internal record ApiGroupMember(long GroupId, long UserId, string Username, bool Bot, long Icon, string Permissions, long RoleId, string CreatedAt, string Type)
    {
       
    }

    internal record ApiGroupRole(int RoleId, string Name, string? Color, string[] Permissions, string[] AllowedCommands);

    internal record ApiGroupServer(long Id, string Name, int SceneIndex, string Status);

    internal record ApiInvitedGroup(
        long Id,
        string? Name,
        string? Description,
        int MemberCount,
        string CreatedAt,
        string Type,
        string[] Tags,
        ApiGroupServer[] Servers,
        int? AllowedServerCount,
        ApiGroupRole[] Roles,
        string InvitedAt) : ApiGroup(Id, Name, Description, MemberCount, CreatedAt, Type, Tags, Servers, AllowedServerCount, Roles);

    internal record ApiJoinedGroup(
        ApiGroup Group,
        ApiGroupMember Member);

    internal record ApiConsoleResponse(long ServerId, bool Allowed, bool WasRejection, bool ColdStart, string FailReason, string? Message, ApiConsoleConnection? Connection, string Token);

    internal record ApiServer(
        long Id,
        string Name,
        ApiServerOnlinePlayerInfo[] OnlinePlayers,
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

    }

    internal record ApiServerOnlinePlayerInfo(long Id, string Username);

    internal record ApiErrorResponse(string Message, string ErrorCode);
}
