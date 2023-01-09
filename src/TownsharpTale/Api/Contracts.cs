namespace Townsharp.Api
{
    public record class ApiConsoleConnection(long ServerId, string Address, string LocalAddress, string PodName, int GamePort, int ConsolePort, int LoggingPort, int WebsocketPort, int WebserverPort);

    public record ApiGroup(long Id, string? Name, string? Description, int MemberCount, string CreatedAt, string Type, string[] Tags, ApiGroupServer[] Servers, int? AllowedServerCount, ApiGroupRole[] Roles)
    {

    };

    public record ApiGroupMember(long GroupId, long UserId, string Username, bool Bot, long Icon, string Permissions, long RoleId, string CreatedAt, string Type)
    {

    }

    public record ApiGroupRole(int RoleId, string Name, string? Color, string[] Permissions, string[] AllowedCommands);

    public record ApiGroupServer(long Id, string Name, int SceneIndex, string Status);

    public record ApiInvitedGroup(
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

    public record ApiJoinedGroup(
        ApiGroup Group,
        ApiGroupMember Member);

    public record ApiConsoleResponse(long ServerId, bool Allowed, bool WasRejection, bool ColdStart, string FailReason, string? Message, ApiConsoleConnection? Connection, string Token);

    public record ApiServer(
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

    public record ApiServerOnlinePlayerInfo(long Id, string Username);

    public record ApiErrorResponse(string Message, string ErrorCode);
}
