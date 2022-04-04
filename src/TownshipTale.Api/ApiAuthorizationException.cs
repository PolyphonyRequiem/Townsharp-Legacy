using System.Runtime.Serialization;

namespace TownshipTale.Api
{
    [Serializable]
    public class ApiAuthorizationException : Exception
    {
        public ApiAuthorizationException()
        {
        }

        public ApiAuthorizationException(string message) : base(message)
        {
        }

        public ApiAuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApiAuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}