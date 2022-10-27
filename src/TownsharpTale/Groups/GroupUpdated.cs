using MediatR;

namespace Townsharp.Groups
{
    public record GroupUpdated(GroupId Id) : INotification;
}
