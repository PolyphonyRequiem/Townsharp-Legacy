using System.Collections.ObjectModel;

namespace Townsharp.Groups
{
    public class Group
    {
        public GroupId Id { get; init; }

        public string Name { get; init; }

        public string Description { get; init; }

        // These are part of the aggregate's responsibility, but not strictly part of it's model.
        //private List<Member> members;
        
        //public ReadOnlyCollection<Member> Members => members.AsReadOnly();

        //private List<Role> roles;

        //public ReadOnlyCollection<Role> Roles => roles.AsReadOnly();

        //public int MemberCount { get; protected set; }

        public GroupType GroupType { get; protected set; }
               
        protected Group(
            GroupId id,
            string name,
            string description,
            GroupType groupType)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.GroupType = groupType;
        }

        public Group(GroupInfo description)
            : this(
                  description.Id,
                  description.Name,
                  description.Description,
                  description.GroupType)
        { }

        // groups can be in a few states at least:
        // Forbidden
        // Doesn't exist (404)
        // Accessible
        // Any difference between invited and joined?  Get help investigating that and record it.
        // Might need a state field here, possibly with "As" methods to promote the view if aligned (this can be extended by the implementation factories)
    }

    public enum GroupType
    {
        Private,
        Public,
        Open
    }
}
