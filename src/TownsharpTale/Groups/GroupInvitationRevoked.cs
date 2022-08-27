using MediatR;

namespace Townsharp.Groups
{
    public record GroupInvitationRevoked(GroupId Id) : INotification;
}
