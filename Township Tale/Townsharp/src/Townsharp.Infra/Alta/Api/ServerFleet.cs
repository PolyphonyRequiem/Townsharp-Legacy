namespace Townsharp.Infra.Alta.Api
{
    public record struct ServerFleet
    {
        private ServerFleet(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public static ServerFleet AttRelease = new ServerFleet("att-release");
        public static ServerFleet AttQuest = new ServerFleet("att-quest");

        static ServerFleet()
        {
            //Values = DiscreteValuesRecordHelpers.GetStaticMappings<ServerFleet>(_ => _.Identifier);
            Values = new Dictionary<string, ServerFleet>();
        }

        private static Dictionary<string, ServerFleet> Values;

        public static implicit operator ServerFleet(string fleet)
        {
            return Values[fleet];
        }

        public static implicit operator string(ServerFleet fleet)
        {
            return fleet.Identifier;
        }
    }
}
