using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TownshipTale.Api.Core.Server.Console;

namespace TownshipTale.Api.Server.Console
{
    public class ConsoleWebsocketClient : IConsoleClient
    {
        private readonly ClientWebSocket clientWebSocket;
        private readonly Task listenerTask;
        private int currentMessageId = 0;
        private readonly byte[] messageBuffer = new byte[16777216];
        private readonly ConsoleConnectionInfo connectionInfo;
        private Dictionary<int, TaskCompletionSource<CommandResult>> pendingMessagesTasks = new Dictionary<int, TaskCompletionSource<CommandResult>>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public ConsoleWebsocketClient(ConsoleConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
            this.clientWebSocket = new ClientWebSocket();
            this.listenerTask = Task.Run(ReceiveContinuously);
        }

        public async Task<CommandResult> ExecuteCommandAsync(Command command, CancellationToken cancellationToken = default)
        {
            await this.EnsureAuthorizedAsync();
            
            return await this.HandleCommandAsync(command, cancellationToken);
        }

        private async Task<CommandResult> HandleCommandAsync(Command command, CancellationToken cancellationToken)
        {
            var id = Interlocked.Increment(ref this.currentMessageId);
            var commandEnvelope = new CommandEnvelope(command, id);

            await SendStringAsync(commandEnvelope.ToString(), cancellationToken);
            var taskCompletionSource = new TaskCompletionSource<CommandResult>();

            try
            {
                pendingMessagesTasks.Add(id, taskCompletionSource);
                return await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                pendingMessagesTasks.Remove(id);
                throw;
            }
        }

        public async Task Disconnect()
        {
            if (this.clientWebSocket.State != WebSocketState.Closed)
            {
                await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "console closed by client.", cancellationToken: default);
                this.cancellationTokenSource.Cancel();
            }
        }

        private async Task ReceiveContinuously()
        {
            var position = 0;

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var rcvBuffer = new ArraySegment<byte>(messageBuffer[position..Math.Max(16777215, position + 64)]);
                WebSocketReceiveResult rcvResult = await this.clientWebSocket.ReceiveAsync(rcvBuffer, cancellationTokenSource.Token);
                byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                position += rcvResult.Count;

                if (rcvResult.EndOfMessage)
                {
                    var document = JsonDocument.Parse(rcvMsg);
                    var result = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });

                    if (document.RootElement.TryGetProperty("type", out JsonElement property))
                    {
                        if (property.GetString() == "CommandResult")
                        {
                            var commandId = document.RootElement.GetProperty("commandId").GetInt32();

                            if (this.pendingMessagesTasks.ContainsKey(commandId))
                            {
                                var commandResult = document.RootElement.GetProperty("data").GetProperty("result");

                                var resultString = JsonSerializer.Serialize(commandResult, new JsonSerializerOptions { WriteIndented = true });
                                this.pendingMessagesTasks[commandId].SetResult(new CommandResult(resultString));
                                this.pendingMessagesTasks.Remove(commandId);
                            }
                        }
                    }

                    System.Console.WriteLine(result);
                }                
            }
        }

        private async Task EnsureAuthorizedAsync()
        {
            if (this.clientWebSocket.State == WebSocketState.Open)
            {
                return;
            }

            var consoleEndpoint = new Uri($"ws://{connectionInfo.IpAddress}:{connectionInfo.WebsocketPort}");
            await this.clientWebSocket.ConnectAsync(consoleEndpoint, CancellationToken.None);
            await this.SendAuthorizationMessageAsync(connectionInfo.Token);
        }

        private async Task SendAuthorizationMessageAsync(string authToken, CancellationToken cancellationToken = default)
        {
            await SendStringAsync(authToken, cancellationToken);
            // expected: {"type":"SystemMessage","timeStamp":"2022-04-03T01:01:48.49774Z","eventType":"InfoLog","data":"Connection Succeeded, Authenticated as: 2026253269 - PolyphonyRequiem"}
            // throw if not received.
        }     

        private async Task SendStringAsync(string message, CancellationToken cancellationToken = default)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            await this.clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
        }       
       
        ~ConsoleWebsocketClient()
        {
            Disconnect().Wait();
        }

        private record CommandEnvelope (Command Command, int Id);
    }
}
