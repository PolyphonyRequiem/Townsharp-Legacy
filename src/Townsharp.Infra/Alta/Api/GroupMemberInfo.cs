namespace Townsharp.Infra.Alta.Api
{
    public record GroupMemberInfo(long GroupId, long UserId, string Username, bool Bot, long Icon, string Permissions, long RoleId, string CreatedAt, string Type);
}
