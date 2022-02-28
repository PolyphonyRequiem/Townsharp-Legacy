namespace TownshipTale.Api.Client
{
    public class ClientCredential
    {
        public const string ClientIdEnvVarKey = "ATT_CLIENT_ID";
        public const string ClientSecretEnvVarKey = "ATT_CLIENT_SECRET";

        public ClientCredential(string clientId, string clientSecret)
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
        }

        /// <summary>
        /// The ClientId to be presented to the Township Tale token issuer.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// The ClientSecret to be presented to the Township Tale token issuer.
        /// </summary>
        public string ClientSecret { get; }

        /// <summary>
        /// Looks for environment variables to populate the <see cref="ClientCredential"/>
        ///   - 'ATT_CLIENT_ID'     : Contains the client id value provided by Alta
        ///   - 'ATT_CLIENT_SECRET' : Contains the client secret value provided by Alta
        /// </summary>
        /// <returns>A populated and validated <see cref="ClientCredential"/> object.</returns>
        public static ClientCredential FromEnvironment()
        {
            return new ClientCredential(
                clientId: Environment.GetRequiredEnvironmentVariable(ClientIdEnvVarKey),
                clientSecret: Environment.GetRequiredEnvironmentVariable(ClientSecretEnvVarKey));
        }
    }
}