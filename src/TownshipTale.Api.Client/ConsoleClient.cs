using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace TownshipTale.Api.Client
{
    public class ConsoleClient
    {
        private readonly uint consoleId;
        private readonly ClientWebSocket clientWebSocket;
        private TaskCompletionSource<ConsoleCommandResult>? taskCompletionSource = default;

        private int sendingId = 0;
        private readonly byte[] messageBuffer = new byte[16777216];

        public ConsoleClient(uint consoleId)
        {
            this.consoleId = consoleId;
            this.clientWebSocket = new ClientWebSocket();
        }

        public async Task<ConsoleCommandResult> SendCommand(string command, CancellationToken cancellationToken = default)
        {
            if (this.clientWebSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("The ConsoleClient has not been connected.  Please call .Connect() first.");
            }

            await SendMessageAsync(command);
            return await ReceiveMessage(cancellationToken);
        }

        public async Task Connect(ConsoleConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            if (this.clientWebSocket.State == WebSocketState.Open)
            {
                return;
            }

            var consoleEndpoint = new Uri($"ws://{connectionInfo.IpAddress}:{connectionInfo.WebsocketPort}");
            await this.clientWebSocket.ConnectAsync(consoleEndpoint, cancellationToken);
            await this.SendAuthorizationMessageAsync(connectionInfo.Token);
        }

        private async Task SendAuthorizationMessageAsync(string authToken, CancellationToken cancellationToken = default)
        {
            await SendStringAsync(authToken, cancellationToken);
            await ReceiveMessage(cancellationToken);
        }

        private async Task SendMessageAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            var envelope = new { Id = ++sendingId, Content = message };
            var messageJson = JsonSerializer.Serialize(envelope);
            await SendStringAsync(messageJson, cancellationToken);
        }

        private async Task SendStringAsync(string message, CancellationToken cancellationToken = default)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            await this.clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
        }

        private async Task<ConsoleCommandResult> ReceiveMessage(CancellationToken cancellationToken = default)
        {
            
            var position = 0;

            while (true)
            {
                var rcvBuffer = new ArraySegment<byte>(messageBuffer[position..Math.Max(16777215, position + 64)]);
                WebSocketReceiveResult rcvResult = await this.clientWebSocket.ReceiveAsync(rcvBuffer, cancellationToken);
                byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                position += rcvResult.Count;

                if (rcvResult.EndOfMessage)
                {
                    var document = JsonDocument.Parse(rcvMsg);
                    var result = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
                    return new ConsoleCommandResult(result);
                }
            }
        }


        public async Task Disconnect()
        {
            if (this.clientWebSocket.State != WebSocketState.Closed)
            {
                await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "console closed by client.", cancellationToken: default);
            }
        }

        ~ConsoleClient()
        {
            Disconnect().Wait();
        }
    }
}