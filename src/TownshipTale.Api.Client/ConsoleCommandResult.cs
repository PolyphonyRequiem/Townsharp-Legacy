namespace TownshipTale.Api.Client
{
    public class ConsoleCommandResult
    {
        public ConsoleCommandResult(string responseContent)
        {
            this.ResponseContent = responseContent;
        }

        public string ResponseContent { get; }
    }
}