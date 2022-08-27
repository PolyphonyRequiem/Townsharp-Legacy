namespace Townsharp.WebApi
{
    public record GroupRoleInfo(int RoleId, string Name, string? Color, string[] Permissions, string[] AllowedCommands);
}