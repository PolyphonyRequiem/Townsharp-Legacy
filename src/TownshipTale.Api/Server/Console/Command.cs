namespace TownshipTale.Api.Server.Console
{
    public class Command
    {
        private string v;

        public Command(string v)
        {
            this.v = v;
        }

        public object CommandString { get; internal set; }
    }
}