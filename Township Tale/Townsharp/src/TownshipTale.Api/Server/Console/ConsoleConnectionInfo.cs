namespace TownshipTale.Api.Server.Console
{
    public readonly record struct ConsoleConnectionInfo(string IpAddress, int WebsocketPort, string Token, int ServerId);
}