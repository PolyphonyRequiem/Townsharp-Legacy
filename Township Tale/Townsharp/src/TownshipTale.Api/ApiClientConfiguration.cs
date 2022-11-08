namespace TownshipTale.Api
{
    public class ApiClientConfiguration
    {
        public ApiClientConfiguration(string clientId)
        {
            ClientId = clientId;
        }

        public string ClientId { get; }
    }
}