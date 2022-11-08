using MediatR;

namespace Townsharp.Groups
{
    public record AddedToGroup(GroupId Id) : INotification;
}
