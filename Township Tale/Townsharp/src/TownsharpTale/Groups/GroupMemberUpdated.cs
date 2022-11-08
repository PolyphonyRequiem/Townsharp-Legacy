using MediatR;
using Townsharp.Users;

namespace Townsharp.Groups
{
    public record GroupMemberUpdated(GroupId Id, UserId UserId) : INotification;
}
