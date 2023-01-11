// See https://aka.ms/new-console-template for more information
using Townsharp.Subscriptions;

Console.WriteLine("Hello, Please give your token");
var token = Console.ReadLine()!;

var client = new SubscriptionClient(() => token);

await client.Run(Connected, Faulted);

client.SubscriptionEventReceived.Subscribe(Console.WriteLine);

//Console.WriteLine("Migration in 1 minute");
//await Task.Delay(60000);
//client.ForceMigration();
//Console.WriteLine("Starting Migration");

void Faulted()
{
    Console.WriteLine("Faulted!");
}

void Connected()
{
    //var subscriptionRequest = new GroupServerStatusSubscription(new GroupId(1896348181));
    //client.Subscribe(subscriptionRequest);
    Task.Run(async () =>
    {
        await client.Subscribe("group-server-status", "1896348181");
        await client.Subscribe("group-server-status", "1156211297");
    });
    
}

await Task.Delay(1000 * 60 * 60);