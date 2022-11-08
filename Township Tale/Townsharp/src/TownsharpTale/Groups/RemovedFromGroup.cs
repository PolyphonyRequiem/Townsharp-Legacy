using MediatR;
using Townsharp.Users;

namespace Townsharp.Groups
{
    public record RemovedFromGroup(GroupId Id, UserId UserId) : INotification;
}
