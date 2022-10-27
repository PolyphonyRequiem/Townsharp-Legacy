using Townsharp.Servers;

namespace Townsharp.Groups
{
    public class Group
    {
        public GroupId Id { get; init; }

        public string Name { get; init; }

        public string Description { get; init; }

        public int LastMemberCount { get; private set; }

        public DateTime CreatedAt { get; init; }

        string Type { get; init; }

        string[] Tags { get; init; }

        Server[] Servers { get; init; }

        int AllowedServerCount { get; init; }

        GroupRole[] Roles { get; init; }

        public Group(GroupId id, string name, string description, int memberCount, DateTime createdAt, string type, List<string> tags, List<Server> servers, int allowedServerCount, GroupRole[] roles)
        {

        }
    }
}
