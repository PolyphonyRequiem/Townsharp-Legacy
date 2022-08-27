namespace Townsharp.WebApi
{
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
}
