using CSharpFunctionalExtensions;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using static Townsharp.Api.Json.JsonUtils;
using Websocket.Client;
using System.Reactive.Subjects;

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
    internal class SubscriptionWebsocketClient : WebsocketClient, IDisposable
    {
        private static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(30);
        private MessageIdFactory messageIdFactory = new MessageIdFactory();
        private readonly Func<string> authorizationTokenFactory;

        private bool disposedValue = false;

        public SubscriptionWebsocketClient(
            Uri url, 
            Func<string> authorizationTokenFactory)
            : base(url, () => AuthorizedClientWebsocketFactory(authorizationTokenFactory))
        {
            this.ReconnectTimeout = null;
            this.authorizationTokenFactory = authorizationTokenFactory;
        }

        private static ClientWebSocket AuthorizedClientWebsocketFactory(Func<string> authorizationTokenFactory)
        {
            var client = new ClientWebSocket();
            client.Options.KeepAliveInterval = TimeSpan.FromMinutes(15);
            client.Options.SetRequestHeader("Authorization", $"Bearer {authorizationTokenFactory.Invoke()}");
            return client;
        }

        // NOTE: inactivity ping is probably needed here.
        public IObservable<SubscriptionEvent> SubscriptionEventReceived =>
            base.MessageReceived
                .Select(AsMaybeSubscriptionResponse)
                .Where(IsSubscriptionEvent)
                .Select(response => AsSubscriptionEvent(response.Value))
                .AsObservable();

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
                    .Select(AsMaybeSubscriptionResponse)
                    .FirstAsync(result => result.HasValue && result.Value.id == id)
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

                // make sure this is a response
                if (response.@event != "response")
                {
                    result = Result.Failure<SubscriptionClientResponse, SubscriptionClientErrorResponse>(
                        new SubscriptionClientErrorResponse(
                            $"A message response for id {id} was received, however it did not contain the expected 'response' event value. '{response.@event}' was received instead.",
                            "UnexpectedResponse"));
                }

                if (response.responseCode >=200 && response.responseCode < 300)
                {
                    result = Result.Success<SubscriptionClientResponse, SubscriptionClientErrorResponse>(
                        new SubscriptionClientResponse(
                            response.key,
                            response.content));
                }
                else
                {
                    result = Result.Failure<SubscriptionClientResponse, SubscriptionClientErrorResponse>(
                        new SubscriptionClientErrorResponse(
                            $"A message response for id {id} was received, however it did not contain a response code indicating success. '{response.responseCode}' was received instead.",
                            "ResponseCodeIndicatesFailure"));
                }
            }
            catch (TimeoutException)
            {
                result = Result.Failure<SubscriptionClientResponse, SubscriptionClientErrorResponse>(
                    new SubscriptionClientErrorResponse(
                        $"Timeout occurred while waiting for message with id {id}",
                        nameof(TimeoutException)));
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private Maybe<SubscriptionResponseMessage> AsMaybeSubscriptionResponse(ResponseMessage responseMessage) => 
            Deserialize<SubscriptionResponseMessage>(responseMessage.Text);

        private bool IsSubscriptionEvent(Maybe<SubscriptionResponseMessage> maybeSubscriptionResponse) => 
            maybeSubscriptionResponse.Match(
                    r =>
                        r.id == 0 &&
                        r.responseCode == 200 &&
                        r.@event != "response",
                    () => false);

        private static bool IsResponseMessageForId(ResponseMessage message, long id) =>
            Deserialize<SubscriptionResponseMessage>(message.Text).Match(
                r => r.@event == "response" && r.id == id,
                () => false);

        private SubscriptionEvent AsSubscriptionEvent(SubscriptionResponseMessage response) =>
            new SubscriptionEvent(response.@event, response.key, response.content);

        private class MessageIdFactory
        {
            long messageId = 0;

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

        private record SubscriptionResponseMessage(long id, string @event, string key, long responseCode, string content)
        {
            public override string ToString() => $"id: {id} event: {@event} key: {key} responseCode: {responseCode} content:{content}";
        }
    }

    internal record SubscriptionClientResponse(string Key, string Content);

    internal record SubscriptionClientErrorResponse(string Message, string ErrorCode);

    internal record SubscriptionEvent(string Event, string Key, string Content);
}
