using System.Runtime.Serialization;

namespace Townsharp.Subscriptions
{
    [Serializable]
    internal class SubscriptionClientMigrationException : Exception
    {
        public SubscriptionClientMigrationException()
            : base("An error occurred during subscription client migration. Scriptions are lost and will need to be requested again.")
        {

        }

        public SubscriptionClientMigrationException(Exception? innerException) 
            : base("An error occurred during subscription client migration. Scriptions are lost and will need to be requested again.", innerException)
        {
        }

        protected SubscriptionClientMigrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}