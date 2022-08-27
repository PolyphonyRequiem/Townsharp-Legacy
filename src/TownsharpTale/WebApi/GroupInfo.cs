namespace Townsharp.WebApi
{
    public record GroupInfo(long Id, string? Name, string? Description, int MemberCount, string CreatedAt, string Type, string[] Tags, GroupServerInfo[] Servers, int? AllowedServerCount, GroupRoleInfo[] Roles);
}
