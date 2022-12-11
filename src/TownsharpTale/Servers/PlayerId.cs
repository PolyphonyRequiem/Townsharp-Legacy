namespace Townsharp.Servers
{

    public record PlayerId
    {
        private readonly long id;

        public PlayerId(long id)
        {
            this.id = id;
        }

        public static implicit operator PlayerId(long id)
            => new PlayerId(id);

        public static implicit operator long(PlayerId playerId)
            => playerId.id;

        public override string ToString()
        {
            return id.ToString();
        }
    }
}