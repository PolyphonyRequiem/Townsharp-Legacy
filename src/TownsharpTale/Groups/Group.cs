using Townsharp.Servers;

namespace Townsharp.Groups
{
    public class Group
    {
        public GroupId Id { get; init; }

        public string Name { get; init; }

        public string Description { get; init; } = string.Empty;

        public int LastMemberCount { get; private set; }

        public DateTime CreatedAt { get; init; }

        string Type { get; init; }

        string[] Tags { get; init; }

        Server[] Servers { get; init; }

        int AllowedServerCount { get; init; }

        GroupRole[] Roles { get; init; }

        public Group(GroupId id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        // groups can be in a few states at least:
        // Forbidden
        // Doesn't exist (404)
        // Accessible
        // Any difference between invited and joined?  Get help investigating that and record it.
    }
}
