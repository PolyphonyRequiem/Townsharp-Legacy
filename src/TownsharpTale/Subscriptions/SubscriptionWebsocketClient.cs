using CSharpFunctionalExtensions;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using static Townsharp.Api.Json.JsonUtils;
using Websocket.Client;

namespace Townsharp.Subscriptions
{
    /// <summary>
    /// 
    /// <remarks>
    /// 
    /// ```
    /// {
    ///     "id": |messageId|,
    ///     "method": "|method|",
    ///     "path": "|path|",
    ///     "authorization": "Bearer |token|"
    /// }
    /// ```
    /// 
    /// A response is of the form:
    /// 
    /// ```
    ///    {
    ///    "id": 1,
    ///    "event": "response",
    ///    "key": "POST /ws/subscription/group-server-status/1156211297",
    ///    "content": "",
    ///    "responseCode": 200
    ///}
    /// ```
    /// 
    /// Errors look like this, note the embedded json:
    /// ```
    /// {
    ///     "id": 1,
    ///     "event": "response",
    ///     "key": "POST /ws/subscription/group-server-status/103278376",
    ///     "content": "{\"message\":\"Could not find any Group with identifier 103278376\",\"error_code\":\"ObjectNotFound\"}",
    ///     "responseCode": 404
    /// }
    /// ```
    /// </remarks>
    /// </summary>
    // Subscription Client should be envelope aware!~
    internal class SubscriptionWebsocketClient : WebsocketClient
    {
        private static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(30);

        private MessageIdFactory messageIdFactory = new MessageIdFactory();
        private readonly Func<string> authorizationTokenFactory;

        public SubscriptionWebsocketClient(Uri url, Func<ClientWebSocket> clientFactory, Func<string> authorizationTokenFactory)
            : base(url, clientFactory)
        {
            this.authorizationTokenFactory = authorizationTokenFactory;
        }

        public SubscriptionWebsocketClient(Uri url, Func<Uri, CancellationToken, Task<WebSocket>> connectionFactory, Func<string> authorizationTokenFactory)
            : base(url, connectionFactory)
        {
            this.authorizationTokenFactory = authorizationTokenFactory;
        }

        // NOTE: Content is basically only used for sending the migration token
        public async Task<Result<SubscriptionClientResponse, SubscriptionClientErrorResponse>> SendRequest(
            HttpMethod method, 
            string path, 
            string content = "", 
            CancellationToken cancellationToken = default)
        {
            var id = this.messageIdFactory.GetNext();

            // this should definitely eventually time out!
            var subscriptionRequestResultTask =
                this.MessageReceived
                    .Select(AsSubscriptionResponse)
                    .FirstAsync(result => result.HasValue && result.Value.Id == id)
                    .Select(result => result.Value)
                    .Timeout(ResponseTimeout)
                    .ToTask(cancellationToken);

            // fabricate the request message.
            var requestMessage = new SubscriptionRequestMessage(id, method, path, this.authorizationTokenFactory.Invoke(), content);

            var requestJsonText = Serialize(requestMessage);
            this.Send(requestJsonText);


            Result<SubscriptionClientResponse, SubscriptionClientErrorResponse> result;

            try
            {
                var response = await subscriptionRequestResultTask;
                result = Result.Success<SubscriptionClientResponse, SubscriptionClientErrorResponse>(new SubscriptionClientResponse("", ""));
            }
            catch (TimeoutException)
            {
                result = Result.Failure<SubscriptionClientResponse, SubscriptionClientErrorResponse>(
                    new SubscriptionClientErrorResponse($"Timeout occurred while waiting for message with id {id}", nameof(TimeoutException)));
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private Maybe<SubscriptionResponseMessage> AsSubscriptionResponse(ResponseMessage responseMessage) => Deserialize<SubscriptionResponseMessage>(responseMessage.Text);

        private static bool IsResponseMessageForId(ResponseMessage message, long id)
        {
            var maybeSubscriptionResponse = Deserialize<SubscriptionResponseMessage>(message.Text);

            if (!maybeSubscriptionResponse.HasValue)
            {
                return false;
            }

            var subscriptionResponse = maybeSubscriptionResponse.Value;

            if (subscriptionResponse.Event != "response")
            {
                return false;
            }

            return subscriptionResponse.Id == id;
        }

        private class MessageIdFactory
        {
            long messageId = 1;

            public long GetNext()
            {
                return Interlocked.Increment(ref messageId);
            }
        }

        private record SubscriptionRequestMessage
        {
            public SubscriptionRequestMessage(long id, HttpMethod method, string path, string token, string content)
            {
                Id = id;
                Method = method.Method;
                Path = path;
                Authorization = $"Bearer {token}";
                Content = content;
            }

            public long Id { get; }

            public string Method { get; }

            public string Path { get; }

            public string Authorization { get; }

            public string Content { get; }

            public override string ToString() => $"id: {Id} method: {Method} path: {Path} content:{Content}";
        }

        private record SubscriptionResponseMessage(long Id, string Event, string Key, int ResponseCode, string Content)
        {
            public override string ToString() => $"id: {Id} event: {Event} key: {Key} responseCode: {ResponseCode} content:{Content}";
        }
    }

    internal record SubscriptionClientResponse(string Key, string Content);

    internal record SubscriptionClientErrorResponse(string Message, string ErrorCode);
}
