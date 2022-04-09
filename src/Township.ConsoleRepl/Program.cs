// See https://aka.ms/new-console-template for more information

using TownshipTale.Api;
using TownshipTale.Api.Core.Server.Console;
using TownshipTale.Api.Identity;
using TownshipTale.Api.Server.Console;

var client = new WebApiClient(new ApiClientConfiguration(args[0]), Authorize);

AccessToken Authorize()
{
    var tokenClient = new TokenClient(new ClientCredential(args[0], args[1]));
    return tokenClient.GetAuthorizationTokenAsync().Result;
}

var connectionInfo = await client.GetConsoleConnectionInfoAsync(1174503463);

var console = new ConsoleWebsocketClient(connectionInfo);

await console.ConnectAsync();

var result = await console.ExecuteCommandAsync(new Command("player message PolyphonyRequiem \"Hello World\""));

var command = String.Empty;

do
{
    Console.Write("?> ");
    command = Console.ReadLine();

    if (command == String.Empty)
    {
        break;
    }

     await console.ExecuteCommandAsync(new Command(command!));
} while (true);

Console.WriteLine(result.ResultContent);