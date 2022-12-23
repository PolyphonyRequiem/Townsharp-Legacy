using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Townsharp;
using Townsharp.Servers;
using Discord;
using Discord.WebSocket;

public class DiscordMarketplace : IHostedService
{
    private readonly Session session;
    private readonly ILogger<DiscordMarketplace> logger;
    private DiscordSocketClient discordSocketClient;

    public DiscordMarketplace(Session session, ILogger<DiscordMarketplace> logger)
    {
        this.session = session;
        this.logger = logger;
        this.discordSocketClient = new DiscordSocketClient();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this.discordSocketClient.Log += message =>
        {
            // handle this properly later, make an adapter.
            this.logger.LogInformation(message.Message);
            return Task.CompletedTask;
        };

        await this.discordSocketClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_POLYPHONY_BOT_TOKEN"));
        await this.discordSocketClient.StartAsync();

        //var server = await session.GetServer(new ServerId(1174503463)); //Cairnbrook
        var servers = await session.GetJoinedServers();
        var server = servers.First(s => s.Id == new ServerId(103278376)); // Mythic tale Quest

        if (server.IsOnline)
        {
            // Eventually do bot stuff, but for now, maybe get the player list.
            var players = server.Players;

            this.logger.LogInformation($"Players Online: {string.Join(", ", players.Select(p => p.Name))}");

            var cairnbrook = this.discordSocketClient.GetGuild(894969628068028477); // Cairnbrook

            var botsChannel = cairnbrook.GetChannel(1039952767525060658);  // cairnbrook-private-bots

            if (botsChannel.GetChannelType() == ChannelType.Text)
            {
                var botsMessageChannel = botsChannel as IMessageChannel;

                await botsMessageChannel!.SendMessageAsync($"Hello World, the Mythic Tale server currently has the following players online: {string.Join(", ", players.Select(p => p.Name))}");
            }
        }

        await Task.Delay(-1);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}