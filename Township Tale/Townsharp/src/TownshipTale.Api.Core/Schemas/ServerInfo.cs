namespace TownshipTale.Api.Core.Api.Schemas
{
    public record ServerInfo (
        long Id, 
        string Name, 
        ServerOnlinePlayers[] OnlinePlayers, 
        string ServerStatus, 
        string FinalStatus, 
        int SceneIndex, 
        int Target, 
        string Region, 
        string? OnlinePing,
        string LastOnline, 
        string Description, 
        int Playability, 
        string Version, 
        long GroupId, 
        string OwnerType,
        long OwnerId, 
        string Type,
        ServerFleet ServerFleet, 
        string Uptime);
}
