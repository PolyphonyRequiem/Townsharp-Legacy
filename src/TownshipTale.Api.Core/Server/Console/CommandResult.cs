namespace TownshipTale.Api.Core.Server.Console
{
    public abstract record CommandResult
    {
        public abstract string ResultContent { get; }
    }
}