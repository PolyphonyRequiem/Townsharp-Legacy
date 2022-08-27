using MediatR;

namespace Townsharp.Groups
{
    public record InvitedToGroup(GroupId Id) : INotification;
}
