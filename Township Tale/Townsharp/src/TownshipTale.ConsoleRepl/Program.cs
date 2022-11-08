using TownshipTale.Api.Client;

ApiClient client = await ApiClient.CreateAuthenticatedClientAsync();

var console = await client.GetConsoleClientAsync(1174503463);

var result = await console.SendCommand("player message PolyphonyRequiem \"Hello\" 10");

Console.WriteLine(result.ResponseContent);
Console.ReadLine();