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
		var serverConnectionInfo = await this.apiClient.GetServerConnectionInfo(serverId);

		if (serverConnectionInfo.Connection == null)
		{
			throw new InvalidOperationException("Server is not available to take connections.");
		}

		return new ConsoleClient(serverId, new Uri($"ws://{serverConnectionInfo.Connection.Address}:{serverConnectionInfo.Connection.WebsocketPort}"), serverConnectionInfo.Token, this.loggerFactory.CreateLogger<ConsoleClient>(), _ => throw new InvalidOperationException("Disconnected!"));
	}
}