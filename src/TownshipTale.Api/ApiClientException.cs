using System.Runtime.Serialization;

namespace TownshipTale.Api
{
    [Serializable]
    internal class ApiClientException : Exception
    {
        public ApiClientException()
        {
        }

        public ApiClientException(string? message) : base(message)
        {
        }

        public ApiClientException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ApiClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}