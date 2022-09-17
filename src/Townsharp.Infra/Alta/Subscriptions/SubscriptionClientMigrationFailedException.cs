using System.Runtime.Serialization;

namespace Townsharp.Infra.Alta.Subscriptions
{
    [Serializable]
    internal class SubscriptionClientMigrationFailedException : Exception
    {
        public SubscriptionClientMigrationFailedException()
        {
        }

        public SubscriptionClientMigrationFailedException(string? message) : base(message)
        {
        }

        public SubscriptionClientMigrationFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SubscriptionClientMigrationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}