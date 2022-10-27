using Townsharp.Servers;
using Microsoft.Extensions.Logging;
using Polly;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using static Townsharp.Infra.Alta.Json.JsonUtils;
using Websocket.Client;
using Websocket.Client.Models;
using System.Text.Json;

namespace Townsharp.Infra.Alta.Console
{
    public class ConsoleClient
    {
        // Constants and invariants
        public readonly static TimeSpan ResponseTimeout = TimeSpan.FromSeconds(15);

        // Transient Fault Handling
        public readonly AsyncPolicy<string> messageRetryPolicy;

        // Dependencies
        private readonly ILogger<ConsoleClient> logger;

        // State
        private readonly ServerId serverId;
        private readonly Uri consoleWebsocketUri;
        private readonly string consoleToken;

        private WebsocketClient? client;
        private bool disposedValue = false;
        private int messageId = 1;
        private ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> subscriptions = new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>();

        // Scheduling and Syncronization
        private SemaphoreSlim creatingWebsocket = new SemaphoreSlim(1, 1);

        // Events, callbacks, and Subscription Sources
        // public IObservable<string> ConsoleDisconnected => ConsoleDisconnectedSubject;
        // private Subject<string> ConsoleDisconnectedSubject = new Subject<string>();
        private readonly Action<string> disconnectedHandler;

        public ConsoleClient(ServerId serverId, Uri consoleWebsocketUri, string consoleToken, ILogger<ConsoleClient> logger, Action<string> disconnectedHandler)
        {
            this.serverId = serverId;
            this.consoleToken = consoleToken;
            this.logger = logger;
            this.disconnectedHandler = disconnectedHandler;

            // Setup our policies
            this.messageRetryPolicy = Policy<string>.Handle<TimeoutException>()
                .RetryAsync(2);

            this.consoleWebsocketUri = consoleWebsocketUri;
        }
        
        public async Task Connect()
        {
            this.client = await this.CreateWebsocketClient();
        }

        public async Task<string> SendCommand(string commandString)
        {
            var result = await this.SendCommand(this.client!, commandString);

            return result.Data?.ToString() ?? String.Empty;
        }

        //private async Task SendSubscriptionRequest(WebsocketClient client, string eventname, string key)
        //{
        //    await messageRetryPolicy.ExecuteAsync(async () => await this.SendMessage(client, HttpMethod.Post, $"subscription/{eventname}/{key}"));
        //}

        //private async Task SendUnscriptionRequest(WebsocketClient client, string eventname, string key)
        //{
        //    await messageRetryPolicy.ExecuteAsync(async () => await this.SendMessage(client, HttpMethod.Delete, $"subscription/{eventname}/{key}"));
        //}

        // Websocket Implementation
        private async Task<WebsocketClient> CreateWebsocketClient()
        {
            logger.LogDebug("Creating a new websocket client");

            var client = new WebsocketClient(this.consoleWebsocketUri)
            {
                Name = $"Console-{this.serverId}-{DateTime.UtcNow}",
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.Zero
            };

            // NOTE: Register general handlers here
            client.MessageReceived.Subscribe(MessageReceived);
            client.DisconnectionHappened.Subscribe(DisconnectionHappened);
            client.ReconnectionHappened.Subscribe(ReconnectionHappened);

            await client.Start();
            TaskCompletionSource tcs = new TaskCompletionSource();

            client
                .MessageReceived
                .Where(response => this.IsAuthenticatedResponse(response)) // NOTE: Get an exact string.
                .Take(1)
                .Timeout(ResponseTimeout)
                .Subscribe(response =>
                {
                    this.logger.LogDebug($"Successfully authenticated.");
                    tcs.SetResult();
                },
                exception =>
                {
                    // given we are only doing this in one place, should we just raise a closed event with reason?
                    this.logger.LogError(exception, $"An error occurred while attempting to authenticate.");
                    tcs.SetException(exception);
                });
            
            client.Send(this.consoleToken);

            await tcs.Task;
            return client;
        }

        private async Task<ConsoleCommandResultMessage> SendCommand(WebsocketClient client, string commandString)
        {
            var id = this.messageId++;
            var request = new ConsoleCommandRequestMessage(id, commandString);

            this.logger.LogDebug($"Sending {request}");

            // Prepare to receive single response.
            TaskCompletionSource<ConsoleCommandResultMessage> tcs = new TaskCompletionSource<ConsoleCommandResultMessage>();

            client
                .MessageReceived
                .Where(response => IsResponseMessageForId(response, id))
                .Take(1)
                .Timeout(ResponseTimeout)
                .Subscribe(response =>
                {
                    try
                    {
                        ConsoleCommandResultMessage commandResponse = Deserialize<ConsoleCommandResultMessage>(response.Text);
                        this.logger.LogDebug($"Got command response for id: {id}.{Environment.NewLine}{commandResponse}");
                        tcs.SetResult(commandResponse);
                    }
                    catch (Exception exc)
                    {
                        this.logger.LogError(exc, $"An error occurred while attempting to process command response for {id}.");
                    }
                },
                exception =>
                {
                    this.logger.LogWarning(exception, $"An error occurred while awaiting response for id: {id}");
                    tcs.SetException(exception);
                });

            var messageText = Serialize(request);
            client.Send(messageText);

            return await tcs.Task;
        }

        private bool IsAuthenticatedResponse(ResponseMessage response)
        {
            try
            {
                var responseMessage = Deserialize<ConsoleResponseMessage>(response.Text);
                if (responseMessage.Data?.RootElement.ToString().Contains("Connection Succeeded, Authenticated as:") ?? false)
                {
                    logger.LogInformation($"Successfully authenticated to console for server {this.serverId}. {responseMessage.Data.RootElement.ToString()}");
                    return true;
                }

                return false;
            }
            catch (Exception) 
            {
                // this is fine, it can happen, we just wanna catch authentication messages.
                return false;
            }
        }

        private bool IsResponseMessageForId(ResponseMessage message, int id)
        {
            var jsonMessage = JsonDocument.Parse(message.Text);
            if (jsonMessage.RootElement.TryGetProperty("type", out var messageTypeElement))
            {
                var messageType = messageTypeElement.GetString();
                if (messageType != "CommandResult")
                {
                    return false;
                }
            }

            if (jsonMessage.RootElement.TryGetProperty("commandId", out var commandId))
            {
                return id == commandId.GetInt32();
            }

            return false;
        }

        private void MessageReceived(ResponseMessage responseMessage)
        {
            this.logger.LogInformation($"RECEIVED: {responseMessage.Text}");
        }

        private void DisconnectionHappened(DisconnectionInfo disconnectionInfo)
        {
            this.logger.LogInformation($"DISCONNECTED: Type-{disconnectionInfo.Type} Status-{disconnectionInfo.CloseStatus} :: {disconnectionInfo.CloseStatusDescription}");

            if (disconnectionInfo.Type == DisconnectionType.Lost)
            {
                // we got dropped.  Something happend Alta side, perhaps a service update.  Either way, we have to assume all of our subscriptions were lost.  Let's initiate recovery.
                //Task.Run(RecoverFaultedConnection);
            }
        }

        private void ReconnectionHappened(ReconnectionInfo reconnectionInfo)
        {
            this.logger.LogInformation($"RECONNECTED: Type-{reconnectionInfo.Type}");
        }

        // DEFINITELY use the event model here that att-client does.

        // open occurs AFTER we get "Connection Succeeded" and have authenticated.

        // check out ping/pong logic

        // connect
        // this.server.status == connecting, type == SystemMessage, eventType == InfoLog data starts with "Connection Succeeded"
        // -> We are connected!

        // send
        // don't use to subscribe or unsubscribe!
        // return command result or error
        // single handler on id.

        // subscribe/unsubscribe
        // prevent dupe sub/invalid unsub
        // send the message
        // register/unregister handler

        // disconnect/dispose

        private record ConsoleResponseMessage
        {
            public string Type { get; init; } = String.Empty;

            public DateTime TimeStamp { get; init; } = DateTime.MinValue;

            public JsonDocument? Data { get; init; } = default;
        }

        private record ConsoleCommandResultMessage : ConsoleResponseMessage
        {
            public int CommandId { get; init; }
        }

        private record ConsoleSystemMessage : ConsoleResponseMessage
        {
            public string EventType { get; init; } = String.Empty;
        }

        private record ConsoleCommandRequestMessage
        {
            public ConsoleCommandRequestMessage(int id, string content)
            {
                if (String.IsNullOrWhiteSpace(content))
                {
                    throw new ArgumentException("Content must not be empty.", nameof(content));
                }

                this.Id = id;
                this.Content = content;
            }

            public int Id { get; init; }

            public string Content { get; init; }
        }
    }
}


//{
//    "server_id": 1174503463,
//    "allowed": true,
//    "was_rejection": false,
//    "cold_start": false,
//    "fail_reason": "Nothing",
//    "connection": {
//        "server_id": 0,
//        "address": "52.54.78.91",
//        "local_address": "127.0.0.1",
//        "pod_name": "att-quest-htlwq-w2gh5",
//        "game_port": 7426,
//        "console_port": 7576,
//        "logging_port": 7796,
//        "websocket_port": 7013,
//        "webserver_port": 7277
//    },
//    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOiI4NjEzMTc4ODEiLCJVc2VybmFtZSI6InBvbHlwaG9ueSIsInNlcnZlcl9pZCI6IjExNzQ1MDM0NjMiLCJleHAiOjE2NjM5NTE4Mjh9.cHyddTT75aeCdNnwO1buJDcijTNkEA60XQQZ2JZesBM"
//}

//"type": "SystemMessage",
//"timeStamp": "2022-09-23T16:47:01.755761Z",
//"eventType": "InfoLog",
//"data": "Connection Succeeded, Authenticated as: 861317881 - polyphony"