namespace TownshipTale.Api.Core.Api.Schemas
{
    public record ServerConnectionInfo(long ServerId, bool Allowed, bool WasRejection, bool ColdStart, string FailReason, string? Message, ConnectionInfo? Connection, string Token);    
}
