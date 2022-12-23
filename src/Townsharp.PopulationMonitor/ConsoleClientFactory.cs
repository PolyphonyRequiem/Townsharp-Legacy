using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Townsharp.Infra.Alta.Api;
using Townsharp.Infra.Alta.Console;
using Townsharp.Servers;

public class ConsoleClientFactory
{
	private readonly ApiClient apiClient;
	private readonly ILoggerFactory loggerFactory;

	public ConsoleClientFactory(ApiClient apiClient, ILoggerFactory loggerFactory)
	{
		this.apiClient = apiClient;
		this.loggerFactory = loggerFactory;
	}

    public async Task<ConsoleClient> CreateClient(ServerId serverId)
    {
        try
        {
            var result = await this.apiClient.RequestConsoleAccess(serverId);

            return new ConsoleClient(serverId, result.Endpoint, result.Token, this.loggerFactory.CreateLogger<ConsoleClient>(), _ => throw new InvalidOperationException("Disconnected!"));
        }
        catch (ServerOfflineException e)
        {
            Debugger.Break();
        }
    }
}