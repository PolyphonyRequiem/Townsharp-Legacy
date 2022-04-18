namespace TownshipTale.Api.Core
{
    public class Server
    {
        // INotify <ServerEvent?>
        // Status
        // Players
        // Events
        // Server Commands

        public int Id { get; set; }

        public string Name { get; }

        public ServerStatus Status { get; }

        IEnumerable<Player> Players { get; }
        
        // Sever events?
        IEnumerable<IEvent> Events { get; }

        IEnumerable<SubscriptionFeed> Subscriptions { get; }

        public void ExecuteCommand(Command command)
        {

        }
    }
}