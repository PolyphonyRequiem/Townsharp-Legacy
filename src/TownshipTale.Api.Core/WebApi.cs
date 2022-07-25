namespace TownshipTale.Api.Core
{
    internal class WebApi
    {
        private ApiClient client;

        public WebApi(ApiClient client)
        {
            this.client = client;
        }

        internal void Authorize()
        {
            throw new NotImplementedException();
        }
    }
}
