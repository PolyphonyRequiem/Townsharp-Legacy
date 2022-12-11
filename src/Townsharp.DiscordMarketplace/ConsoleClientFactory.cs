using Microsoft.Extensions.Logging;
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
		var consoleInfo = await this.apiClient.GetConsoleInfo(serverId);

		if (consoleInfo.Connection == null)
		{
			throw new InvalidOperationException("Server is not available to take connections.");
		}

		return new ConsoleClient(serverId, new Uri($"ws://{consoleInfo.Connection.Address}:{consoleInfo.Connection.WebsocketPort}"), consoleInfo.Token, this.loggerFactory.CreateLogger<ConsoleClient>(), _ => throw new InvalidOperationException("Disconnected!"));
	}
}