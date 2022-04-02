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
    }
}