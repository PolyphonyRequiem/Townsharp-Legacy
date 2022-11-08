namespace Townsharp.Infra.Alta.Api
{
    public record ServerInfo(
        long Id,
        string Name,
        ServerOnlinePlayers[] OnlinePlayers,
        string ServerStatus,
        string FinalStatus,
        int SceneIndex,
        int Target,
        string Region,
        string? OnlinePing,
        DateTime LastOnline,
        string Description,
        float Playability,
        string Version,
        long GroupId,
        string OwnerType,
        long OwnerId,
        string Type,
        string JoinType,
        ServerFleet ServerFleet,
        TimeSpan UpTime);
}
