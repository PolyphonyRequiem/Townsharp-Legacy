using TownshipTale.Api.Core.Api.Schemas;

namespace TownshipTale.Api.Core
{
    public record ClientConfiguration(
        long[] ExcludedGroups,
        long[] IncludedGroups,
        int MaxWorkerConcurrency,
        Uri RestBaseUrl,
        Scope[] Scopes,
        TimeSpan ServerConnectionRecoveryDelay,
        TimeSpan ServerHeartbestTimeout,
        ServerFleet[] SupportedServerFleets,
        Uri TokenUrl,
        TimeSpan WebSocketMigrationHandoverPeriod,
        TimeSpan WebSocketMigrationInterval,
        TimeSpan WebSocketMigrationRetryDelay,
        TimeSpan WebSocketPingInterval,
        TimeSpan WebSocketRecoveryRetryDelay,
        int WebSocketRecoveryTimeout,
        int WebSocketRequestAttempts,
        TimeSpan WebSocketRequestRetryDelay,
        Uri WebSocketUrl,
        string XApiKey)
    {

    }
}