using Townsharp.Users;

namespace Townsharp.Groups.Users
{
    public class User
    {
        public UserId Id { get; init; }

        public User(UserId id)
        {
            Id = id;
        }
    }
}
