namespace Townsharp.Users
{
    public record UserId
    {
        private readonly long id;

        public UserId(long id)
        {
            this.id = id;
        }

        public static implicit operator UserId(long id)
            => new UserId(id);

        public static implicit operator long(UserId userId)
            => userId.id;
    }
}
