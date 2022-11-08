namespace TownshipTale.Api.Core.Api.Schemas
{
    public record GroupMemberInfo (long GroupId, long UserId, string? Username, bool Bot, long Icon, string Permissions, long RoleId, string CreatedAt, string Type);
}
