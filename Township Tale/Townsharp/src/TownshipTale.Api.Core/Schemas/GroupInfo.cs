namespace TownshipTale.Api.Core.Api.Schemas
{
    // CreatedAt might want to be a different type?  But since this is a contract, I'm not too fussed about it.
    public record GroupInfo(long Id, string? Name, string? Description, int MemberCount, string CreatedAt, string Type, string[] Tags, GroupServerInfo[] Servers, int? AllowedServerCount, GroupRoleInfo[] Roles);
}
