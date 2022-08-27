using Townsharp.Servers;
using Townsharp.Servers.Consoles;

namespace Townsharp
{
    public record ConsoleConnected(ServerId ServerId, ConsoleSession ConsoleSession);
}
