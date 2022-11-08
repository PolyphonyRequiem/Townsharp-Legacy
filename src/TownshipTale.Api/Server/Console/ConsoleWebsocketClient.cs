using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace TownshipTale.Api.Server.Console
{
    public class ConsoleWebsocketClient : IConsoleClient
    {
        private readonly ClientWebSocket clientWebSocket;
        private Task listenerTask;
        private int currentMessageId = 0;
        private readonly byte[] messageBuffer = new byte[16777216];
        private readonly ConsoleConnectionInfo connectionInfo;
        private Dictionary<int, TaskCompletionSource<CommandResult>> pendingMessagesTasks = new Dictionary<int, TaskCompletionSource<CommandResult>>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private bool isConnected;

        public ConsoleWebsocketClient(ConsoleConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
            this.clientWebSocket = new ClientWebSocket();
        }

        public async Task ConnectAsync()
        {
            if (this.isConnected)
            {
                return;
            }


            var consoleEndpoint = new Uri($"ws://{connectionInfo.IpAddress}:{connectionInfo.WebsocketPort}");
            await this.clientWebSocket.ConnectAsync(consoleEndpoint, CancellationToken.None);

            this.listenerTask = Task.Run(ReceiveContinuously);

            await this.SendAuthorizationMessageAsync(connectionInfo.Token);
            this.isConnected = true;

            // TODO No, not this... seriously.
            await Task.Delay(1000);
            // we should await response though going forward.

            // Nope, you need an "open" for this of some sort champ.  

            await this.ExecuteCommandAsync(new Command("websocket subscribe InfoLog"));
            await this.ExecuteCommandAsync(new Command("websocket subscribe TraceLog"));

            
        }

        public async Task<CommandResult> ExecuteCommandAsync(Command command, CancellationToken cancellationToken = default)
        {
            if (this.isConnected == false)
            {
                throw new InvalidOperationException("Connect first");
            }

            return await this.HandleCommandAsync(command, cancellationToken);
        }

        private async Task<CommandResult> HandleCommandAsync(Command command, CancellationToken cancellationToken)
        {
            var id = Interlocked.Increment(ref this.currentMessageId);
            var commandEnvelope = new CommandEnvelope(command, id);

            var taskCompletionSource = new TaskCompletionSource<CommandResult>();
            pendingMessagesTasks.Add(id, taskCompletionSource);

            await SendStringAsync(commandEnvelope.ToString(), cancellationToken);

            try
            {
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

            var rcvBuffer = new ArraySegment<byte>(messageBuffer[position..Math.Max(16777215, position + 64)]);
            while (!cancellationTokenSource.IsCancellationRequested)
            {                
                WebSocketReceiveResult rcvResult = await this.clientWebSocket.ReceiveAsync(rcvBuffer, cancellationTokenSource.Token);
                byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                position += rcvResult.Count;

                if (rcvResult.EndOfMessage)
                {
                    var document = JsonDocument.Parse(rcvMsg);
                    var result = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
                    System.Console.WriteLine(result);

                    if (document.RootElement.TryGetProperty("type", out JsonElement property))
                    {
                        if (property.GetString() == "CommandResult")
                        {
                            var commandId = document.RootElement.GetProperty("commandId").GetInt32();

                            if (this.pendingMessagesTasks.ContainsKey(commandId))
                            {
                                var commandData = document.RootElement.GetProperty("data");
                                string resultString = String.Empty;

                                if (commandData.GetProperty("Command").GetProperty("ReturnType").GetString() == "System.Void")
                                {
                                    resultString = commandData.GetProperty("ResultString").GetString();
                                }
                                else
                                {
                                    var commandResult = commandData.GetProperty("Result").GetString();
                                    resultString = JsonSerializer.Serialize(commandResult, new JsonSerializerOptions { WriteIndented = true });
                                }
                                
                                this.pendingMessagesTasks[commandId].SetResult(new CommandResult(resultString ?? "N/A"));
                                this.pendingMessagesTasks.Remove(commandId);
                            }
                        }
                    }

                    position = 0;
                }                
            }
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

        private record CommandEnvelope (Command command, int id)
        {
            public override string ToString()
            {
                return JsonSerializer.Serialize(new { content = command.CommandString, id });
            }
        }
    }
}
