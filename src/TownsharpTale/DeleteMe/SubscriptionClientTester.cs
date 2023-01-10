using Townsharp.Subscriptions;

namespace Townsharp.DeleteMe
{
    public class SubscriptionClientTester
    {
        private SubscriptionWebsocketClient client;

        public SubscriptionClientTester(Func<string> tokenFactory)
        {
            this.client = new SubscriptionWebsocketClient(new Uri("wss://5wx2mgoj95.execute-api.ap-southeast-2.amazonaws.com/dev"), tokenFactory);
            client.SubscriptionEventReceived.Subscribe(e => Console.WriteLine($"EVENT: {e}"));
            client.MessageReceived.Subscribe(e => Console.WriteLine($"RAW: {e.Text}"));
            client.DisconnectionHappened.Subscribe(e => Console.WriteLine($"DCON: {e.CloseStatus} {e.CloseStatusDescription}"));
            client.ReconnectionHappened.Subscribe(e => Console.WriteLine($"RCON: {e.Type}"));
            client.Start();
        }

        public async Task Subscribe()
        {
            await client.SendRequest(HttpMethod.Post, "subscription/group-server-status/1896348181");
            await client.SendRequest(HttpMethod.Post, "subscription/group-server-status/1156211297");
        }
    }
}