namespace Townsharp.Servers
{
    public record ServerId
    {
        private readonly long id;

        public ServerId(long id)
        {
            this.id = id;
        }

        public static implicit operator ServerId(long id)
            => new ServerId(id);

        public static implicit operator long(ServerId serverId)
            => serverId.id;
    }
}
