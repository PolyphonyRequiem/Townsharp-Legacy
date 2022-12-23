namespace Townsharp.Groups
{
    public record Role
    {
        RoleId Id { get; init; }

        string Name { get; init; }

        // string Color { get; init; }

        string[] Permissions { get; init; }

        // string[] AllowedCommands { get; init; }

        public Role(RoleId id, string name, IList<string> permissions)
        {
            Id = id;
            Name = name;
            Permissions = permissions.ToArray();
        }
    }
}