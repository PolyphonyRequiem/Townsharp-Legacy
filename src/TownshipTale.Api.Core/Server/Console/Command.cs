namespace TownshipTale.Api.Core.Server.Console
{

    public abstract record CommandWithArguments : Command
    {
        public abstract string[] Arguments { get; }

        public override string Statement => $"{base.Statement} {String.Join(' ', Arguments)}";
    }

    public abstract record Command
    {
        public abstract string CommandGroup { get; }

        public abstract string CommandAction { get; }

        public virtual string Statement => $"{CommandGroup} {CommandAction}";

        public override string ToString() => this.Statement;
    }
}