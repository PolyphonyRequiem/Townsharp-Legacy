// See https://aka.ms/new-console-template for more information
using Townsharp.DeleteMe;

Console.WriteLine("Hello, Please give your token");
var token = Console.ReadLine()!;

var tester = new SubscriptionClientTester(() => token);

await tester.Subscribe();

await Task.Delay(1000 * 60 * 60);