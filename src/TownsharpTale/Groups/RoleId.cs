namespace Townsharp.Groups
{
    public record RoleId
    {
        private readonly long id;

        public RoleId(long id)
        {
            this.id = id;
        }

        public static implicit operator RoleId(long id)
            => new RoleId(id);

        public static implicit operator long(RoleId roleId)
            => roleId.id;
    }
}