namespace Townsharp
{
    public class TownsharpConfig
    {
        public int MaxDegreeOfParallelism { get; init; } = 8;

        public bool AutoManageJoinedGroups { get; init; } = true;

        public bool AutoManageJoinedServers { get; init; } = true;

        public bool AutoJoinInvitedGroups { get; init; } = true;

        public static TownsharpConfig Default = new TownsharpConfig();
    }
}