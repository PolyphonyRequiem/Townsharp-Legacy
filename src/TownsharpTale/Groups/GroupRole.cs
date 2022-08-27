namespace Townsharp.Groups
{
    public record GroupRole
    {
        RoleId Id { get; init; }

        string Name { get; init; }

        // string Color { get; init; }

        string[] Permissions { get; init; }

        // string[] AllowedCommands { get; init; }

        public GroupRole(RoleId id, string name, IList<string> permissions)
        {
            Id = id;
            Name = name;
            Permissions = permissions.ToArray();
        }
    }
}