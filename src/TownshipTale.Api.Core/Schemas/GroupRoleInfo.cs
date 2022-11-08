namespace TownshipTale.Api.Core.Api.Schemas
{
    public record GroupRoleInfo (int RoleId, string? Name, string? Color, string[] Permissions, string[] AllowedCommands);
}