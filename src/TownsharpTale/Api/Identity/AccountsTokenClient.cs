using Duende.AccessTokenManagement;
using Townsharp.Api.Composition;

namespace Townsharp.Api.Identity
{
    public class AccountsTokenClient
    {
        public const string BaseUri = "https://accounts.townshiptale.com/connect/token";

        public const string DefaultScopes = "ws.group ws.group_members ws.group_servers ws.group_bans ws.group_invites group.info group.join group.leave group.view group.members group.invite server.view server.console";

        private readonly IClientCredentialsTokenManagementService tokenManagementService;

        public AccountsTokenClient(IClientCredentialsTokenManagementService tokenManagementService)
        {
            this.tokenManagementService = tokenManagementService;
        }

        public async Task<ClientCredentialsToken> GetValidToken(bool forceRenewal = false) =>
          await tokenManagementService.GetAccessTokenAsync(TokenManagementNames.AccountsIssuer, new TokenRequestParameters { ForceRenewal = forceRenewal });

    }
}
