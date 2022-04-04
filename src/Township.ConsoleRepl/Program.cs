// See https://aka.ms/new-console-template for more information

using TownshipTale.Api;
using TownshipTale.Api.Core.Server.Console;
using TownshipTale.Api.Identity;
using TownshipTale.Api.Server.Console;

Console.WriteLine("Hello, World!");

var client = new WebApiClient(new ApiClientConfiguration(args[0]), Authorize);

AccessToken Authorize()
{
    var tokenClient = new TokenClient(new ClientCredential(args[0], args[1]));

    return tokenClient.GetAuthorizationTokenAsync().Result;
}

var console = new ConsoleWebsocketClient(new ConsoleConnectionInfo("44.199.250.194", 7395, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOiI4NjEzMTc4ODEiLCJVc2VybmFtZSI6InBvbHlwaG9ueSIsInNlcnZlcl9pZCI6IjExNzQ1MDM0NjMiLCJleHAiOjE2NDg5NTYxOTh9.zNHFubD8NUXQqx9BwBsK2uv_EfQYF8UZxbaQpLbkoNU", 1174503463));

var result = await console.ExecuteCommandAsync(new Command("player message PolyphonyRequiem \"Hello World\""));

Console.WriteLine(result);