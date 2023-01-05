using Townsharp.Groups;
using Townsharp.Users;
using Townsharp.Consoles;

namespace Townsharp.Servers
{
    public interface IServerService
    {
        IAsyncEnumerable<ServerInfo> GetJoinedServerDescriptions();

        Task<ServerInfo> GetServerDescription(ServerId serverId);

        Task<ConsoleAccessResult> RequestConsoleAccess(ServerId serverId);
    }
}
