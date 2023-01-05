using System.Runtime.Serialization;

namespace Townsharp
{
    [Serializable]
    internal class SessionNotReadyException : Exception
    {
        public SessionNotReadyException()
        {
        }

        public SessionNotReadyException(string? message) : base(message)
        {
        }

        public SessionNotReadyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SessionNotReadyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}