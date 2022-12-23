//// See https://aka.ms/new-console-template for more information

//using TownshipTale.Api;
//using TownshipTale.Api.Server.Console;

//var client = new WebApiClient(new ApiClientConfiguration(args[0]), Authorize);

//AccessToken Authorize()
//{
//    var tokenClient = new TokenClient(new ClientCredential(args[0], args[1]));
//    return tokenClient.GetAuthorizationTokenAsync().Result;
//}

//var connectionInfo = await client.GetConsoleConnectionInfoAsync(1174503463);

//var console = new ConsoleWebsocketClient(connectionInfo);

//await console.ConnectAsync();

//var result = await console.ExecuteCommandAsync(new Command("player message PolyphonyRequiem \"Hello World\""));

//var command = String.Empty;

//do
//{
//    Console.Write("?> ");
//    command = Console.ReadLine();

//    if (command == String.Empty)
//    {
//        break;
//    }

//     await console.ExecuteCommandAsync(new Command(command!));
//} while (true);

//Console.WriteLine(result.ResultContent);


var result = Result<bool, int>.Ok(true)
    .Match(
        value => value.ToString()/*mustbetrue*/,
        errorCode => errorCode.ToString());

Console.WriteLine(result);

public abstract class Result<TValue, TError>
{
    public abstract TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> error);
    public abstract Result<TResult, TError> Map<TResult>(Func<TValue, TResult> f);
    public abstract Result<TResult, TError> Bind<TResult>(Func<TValue, Result<TResult, TError>> f);

    public static Result<TValue, TError> Ok(TValue value) => new Success<TValue, TError>(value);
    public static Result<TValue, TError> Error(TError error) => new Error<TValue, TError>(error);
}

public class Success<TValue, TError> : Result<TValue, TError>
{
    private readonly TValue _value;

    public Success(TValue value)
    {
        _value = value;
    }

    public override TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> error)
        => success(_value);

    public override Result<TResult, TError> Map<TResult>(Func<TValue, TResult> f)
        => new Success<TResult, TError>(f(_value));

    public override Result<TResult, TError> Bind<TResult>(Func<TValue, Result<TResult, TError>> f)
        => f(_value);
}

public class Error<TValue, TError> : Result<TValue, TError>
{
    private readonly TError _error;

    public Error(TError error)
    {
        _error = error;
    }

    public override TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> error)
        => error(_error);

    public override Result<TResult, TError> Map<TResult>(Func<TValue, TResult> f)
        => this as Result<TResult, TError>;

    public override Result<TResult, TError> Bind<TResult>(Func<TValue, Result<TResult, TError>> f)
        => this as Result<TResult, TError>;
}