
namespace TownshipTale.Api.Core
{
    public record Scope
    {
        private Scope(string scope)
        {
            Value = scope;
        }

        public string Value { get; }

        public static Scope GroupInfo = new Scope("group.info");
        public static Scope GroupInvite = new Scope("group.invite");
        public static Scope GroupJoin = new Scope("group.join");
        public static Scope GroupLeave = new Scope("group.leave");
        public static Scope GroupMembers = new Scope("group.members");
        public static Scope GroupView = new Scope("group.view");

        static Scope()
        {
            Values = new Dictionary<string, Scope>();
            //Values = Values = DiscreteValuesRecordHelpers.GetStaticMappings<Scope>(_ => _.Value);
        }

        private static Dictionary<string, Scope> Values;

        public static implicit operator Scope(string scope)
        {
            return Values[scope];
        }

        public static implicit operator string(Scope scope)
        {
            return scope.Value;
        }
    }
}