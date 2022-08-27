using Microsoft.Extensions.Logging;
using Polly;
using System.IdentityModel.Tokens.Jwt;
using TownshipTale.Api.Core.Api.Schemas;

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

            var userid = token.Claims.Single(c=>c.Value == "client_sub").Value;

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
                    this.subscriptionManager.Subscribe<MeGroupInviteCreateMessage>(SubscriptionClientEvent.MeGroupInviteCreate, userid, async message =>
                    {
                        this.logger.LogInformation($"Accepting invite to group {message.Content.Id} ({message.Content.Name}");
                        await this.api.AcceptGroupInviteAsync(message.Content.Id);
                    }),
                    this.subscriptionManager.Subscribe<MeGroupCreateMessage>(SubscriptionClientEvent.MeGroupCreate, userid, async message =>
                    {            
                        // The group info from this message is missing information about
                        // this group's servers and roles. So we'll use the group ID from
                        // this message to fetch more complete information. We'll also
                        // need to get this client's group membership details to determine
                        // group permissions.  

                        var groupId = message.Content.Id;
                        var groupName = message.Content.Name;

                        this.logger.LogInformation($"Client was added to group {groupId} ({groupName}).");
                        
                        GroupInfo group = await this.api.GetGroupInfoAsync(groupId);
                        GroupMemberInfo member = await this.api.GetGroupMemberAsync(groupId, userid);


                        // NOTE: Possibly replace with null checks?
                        //if (typeof group === 'undefined')
                        //{
                        //    this.logger.error(`Couldn't get info for group ${groupId} (${groupName}).`);
                        //  return;
                        //}

                        //if (typeof member === 'undefined')
                        //{
                        //    this.logger.error(`Couldn't find group member info for group ${group.id} (${groupName}).`);
                        //  return;
                        //}

                        // Create a new managed group.
                        this.AddGroup(group, member);
                    }),
                    this.subscriptionManager.Subscribe<MeGroupDeleteMessage>(SubscriptionClientEvent.MeGroupDelete, userid, message =>
                    {
                        var groupId = message.Content.Group.Id;
                        var groupName = message.Content.Group.Name;

                        this.logger.LogInformation($"Client was removed from group {groupId} ({groupName}).");
                        this.RemoveGroupAsync(groupId);
                    })

                };

                await Task.WhenAll(subscriptionTasks);

                var joinedGroups = await this.api.GetJoinedGroupsAsync();

                if (joinedGroups.Length > 0)
                {
                    this.logger.LogInformation($"Managing {joinedGroups.Length} group{(joinedGroups.Length > 1 ? "s" : "")}.");

                    Parallel.ForEach(joinedGroups, new ParallelOptions { MaxDegreeOfParallelism = configuration.MaxWorkerConcurrency }, joinedGroup =>
                        this.AddGroup(joinedGroup.Group, joinedGroup.Member));
                }

            }
            catch (Exception ex)
            {
                // TODO: Not this.
                throw;
            }
        }

        private void RemoveGroupAsync(long groupId)
        {
            throw new NotImplementedException();
        }

        private void RemoveGroup(long groupId)
        {
            throw new NotImplementedException();
        }

        private void AddGroup(GroupInfo group, GroupMemberInfo member)
        {
            throw new NotImplementedException();
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
