using System.Runtime.Serialization;

namespace Townsharp.Infra.Alta.Subscriptions
{
    [Serializable]
    internal class AltaSubscriptionClientException : Exception
    {
        public string ErrorCode { get; init; }

        public AltaSubscriptionClientException(string message, string errorCode) : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public AltaSubscriptionClientException(string? message, string errorCode, Exception? innerException) : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public AltaSubscriptionClientException(string? message, Exception? innerException) : base(message, innerException)
        {
            this.ErrorCode = String.Empty;
        }

        protected AltaSubscriptionClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ErrorCode = String.Empty;
        }
    }
}