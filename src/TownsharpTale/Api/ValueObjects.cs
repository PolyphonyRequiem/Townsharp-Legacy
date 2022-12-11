

using Townsharp.Groups;

namespace Townsharp.Api
{
    // should these be hidden within the client?

    public record class ConnectionInfo(long ServerId, string Address, string LocalAddress, string PodName, int GamePort, int ConsolePort, int LoggingPort, int WebsocketPort, int WebserverPort);
    public record GroupInfo(long Id, string? Name, string? Description, int MemberCount, string CreatedAt, string Type, string[] Tags, GroupServerInfo[] Servers, int? AllowedServerCount, GroupRoleInfo[] Roles);
    public record GroupMemberInfo(long GroupId, long UserId, string Username, bool Bot, long Icon, string Permissions, long RoleId, string CreatedAt, string Type);
    public record GroupRoleInfo(int RoleId, string Name, string? Color, string[] Permissions, string[] AllowedCommands);
    public record GroupServerInfo(long Id, string Name, int SceneIndex, string Status);

    public record InvitedGroupInfo(
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

    public record JoinedGroupInfo(
        GroupInfo Group,
        GroupMemberInfo Member);

    public record ConsoleSessionInfo(long ServerId, bool Allowed, bool WasRejection, bool ColdStart, string FailReason, string? Message, ConnectionInfo? Connection, string Token);

    public record struct ServerFleet
    {
        private ServerFleet(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public static ServerFleet AttRelease = new ServerFleet("att-release");
        public static ServerFleet AttQuest = new ServerFleet("att-quest");

        static ServerFleet()
        {
            //Values = DiscreteValuesRecordHelpers.GetStaticMappings<ServerFleet>(_ => _.Identifier);
            Values = new Dictionary<string, ServerFleet>();
        }

        private static Dictionary<string, ServerFleet> Values;

        public static implicit operator ServerFleet(string fleet)
        {
            return Values[fleet];
        }

        public static implicit operator string(ServerFleet fleet)
        {
            return fleet.Identifier;
        }
    }

    public record ServerInfo(
        long Id,
        string Name,
        ServerOnlinePlayer[] OnlinePlayers,
        string ServerStatus,
        string FinalStatus,
        int SceneIndex,
        int Target,
        string Region,
        string? OnlinePing,
        DateTime LastOnline,
        string Description,
        float Playability,
        string Version,
        long GroupId,
        string OwnerType,
        long OwnerId,
        string Type,
        string JoinType,
        ServerFleet ServerFleet,
        TimeSpan UpTime,
        bool IsOnline);

    public record ServerOnlinePlayer(long Id, string Username);
}
