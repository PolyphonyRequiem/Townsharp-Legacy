using TownshipTale.Api.Core.Api.Schemas;

namespace TownshipTale.Api.Core
{
    public class Constants
    {
        public int MAX_WORKER_CONCURRENCY = 5;

        public int MAX_WORKER_CONCURRENCY_WARNING = 10;

        public string REST_BASE_URL = "https://967phuchye.execute-api.ap-southeast-2.amazonaws.com/prod/api";

        public TimeSpan SERVER_CONNECTION_RECOVERY_DELAY = TimeSpan.FromSeconds(10);

        public TimeSpan SERVER_HEARTBEAT_TIMEOUT = TimeSpan.FromMinutes(10);

        public ServerFleet[] SUPPORTED_SERVER_FLEETS = new[] { ServerFleet.AttQuest, ServerFleet.AttRelease };

        public string TOKEN_URL = "https://accounts.townshiptale.com/connect/token";

        public TimeSpan WEBSOCKET_MIGRATION_HANDOVER_PERIOD = TimeSpan.FromSeconds(10);

        public TimeSpan WEBSOCKET_MIGRATION_INTERVAL = TimeSpan.FromMinutes(110);

        public TimeSpan WEBSOCKET_MIGRATION_RETRY_DELAY = TimeSpan.FromSeconds(10);

        public TimeSpan WEBSOCKET_PING_INTERVAL = TimeSpan.FromMinutes(5);

        public TimeSpan WEBSOCKET_RECOVERY_RETRY_DELAY = TimeSpan.FromSeconds(5);

        public int WEBSOCKET_RECOVERY_TIMEOUT = 2;

        public int WEBSOCKET_REQUEST_ATTEMPTS = 3;

        public TimeSpan WEBSOCKET_REQUEST_RETRY_DELAY = TimeSpan.FromSeconds(3);

        public string WEBSOCKET_URL = "wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev";

        public string X_API_KEY = "2l6aQGoNes8EHb94qMhqQ5m2iaiOM9666oDTPORf";
    }
}
