using Microsoft.Extensions.Logging;
using Polly;
using System.IdentityModel.Tokens.Jwt;

namespace TownshipTale.Api.Core
{
    public class ApiClient
    {
        private readonly ClientConfiguration configuration;
        private readonly ILogger<ApiClient> logger;
        private readonly WebApi api;
        private readonly SubscriptionManager subscriptionManager;
        private readonly Dictionary<long, Group> groups = new Dictionary<long, Group>();
        private readonly TokenClient tokenClient;
        private bool isInitialized = false;

        public ApiClient(TokenClient tokenClient, ClientConfiguration configuration, ILogger<ApiClient> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            // TODO: If no logger is configured, use the console logger.
            // TODO: Verbosity Deferred to Configuration as well.  If unconfigured user Warning.
            // TODO: ValidateConfiguration
            this.api = new WebApi(this);
            this.tokenClient = tokenClient;
            this.subscriptionManager = new SubscriptionManager(this);
        }

        public async Task InitializeAsync(ClientConnectHandler connectHandler, ClientReadyHandler readyHandler)
        {
            if (this.isInitialized)
            {
                throw new InvalidOperationException("This client is already initialized.");                
            }

            this.isInitialized = true;

            this.logger.LogInformation("Initializing client.");

            var token = await this.RefreshAccessTokenAsync();

            var userid = long.Parse(token.Claims.Single(c=>c.Value == "client_sub").Value);

            // Initialize subscriptions
            this.logger.LogInformation("Subscribing to events.");

            await this.subscriptionManager.InitializeSubscriptionsAsync();

            try
            {
                // Subscribe to account messages.
                this.logger.LogDebug("Subscribing to account messages.");

                var subscriptionTasks = new Task[]
                {
                    // NOTE: This pattern doesn't make sense in C#.  Let's pivot later.
                    this.subscriptionManager.Subscribe<MeGroupInviteCreateMessage>(SubscriptionClientEvent.MeGroupInviteCreate, userid, message =>
                    {
                        // NOTE: Wait, why is this double content? TIME TO STOP HERE FOR THE NIGHT FOR SURE!!!
                        this.logger.LogInformation($"Accepting invite to group {message.Content.Id} ({message.Content.Content.Name}");
                        this.api.AcceptGroupInvite(id);
                    })
                };

                await Task.WhenAll(subscriptionTasks);
            }
        }

        public async Task<JwtSecurityToken> RefreshAccessTokenAsync()
        {
            // Retrieve and decode our Access Token
            var accessToken = await this.GetAccessTokenAsync();
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(accessToken.Token);

            // Reauthorize the API interface
            this.api.Authorize();

            // Schedule our refresh
            // TODO: Implement this

            return jwt;
        }

        private async Task<AccessToken> GetAccessTokenAsync()
        {
            this.logger.LogInformation("Retrieving access token.");
            return await this.tokenClient.GetAuthorizationTokenAsync();
        }
    }


    public delegate void ClientConnectHandler(ServerConnection serverConnection);

    public delegate void ClientReadyHandler();
}
