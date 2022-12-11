using Townsharp.Infra.Alta.Console;

internal class ManagedConsole
{
    private ConsoleClient consoleClient;
    private ManagedConsoleStatus uninitialized;

    public ManagedConsole(ConsoleClient consoleClient, ManagedConsoleStatus uninitialized)
    {
        this.consoleClient = consoleClient;
        this.uninitialized = uninitialized;
    }
}