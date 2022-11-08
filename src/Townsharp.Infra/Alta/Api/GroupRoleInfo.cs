namespace Townsharp.Infra.Alta.Api
{
    public record GroupRoleInfo(int RoleId, string Name, string? Color, string[] Permissions, string[] AllowedCommands);
}