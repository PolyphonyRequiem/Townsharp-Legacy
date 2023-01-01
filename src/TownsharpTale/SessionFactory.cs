namespace Townsharp
{
    public class SessionFactory<TSession>
        where TSession : Session
    {
        private readonly Func<TownsharpConfig, TSession> createSession;

        public SessionFactory(Func<TownsharpConfig, TSession> createSession) 
        {
            this.createSession = createSession;
        }

        public Session Create(
            TownsharpConfig config)
        {
            return createSession(config);
        }
    }
}
