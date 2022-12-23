using Townsharp.Groups;
using System.Reactive.Subjects;

namespace Townsharp.Servers
{
    public class Server
    {
        public ServerId Id { get; }

        public GroupId GroupId { get; }

        public bool IsOnline { get; protected set; }

        // noope, readonly collection by way of an accessor please.
        public Player[] Players { get; protected set; }

        internal Server(
            ServerId id,
            GroupId groupId,
            bool isOnline,
            Player[] players)
        {
            Id = id;
            GroupId = groupId;
            IsOnline = isOnline;
            Players = players;
        }

        // NOTE: Inject service dependencies at the method invocation level, it's easier to test and makes the dependency use cases more clear.

        protected internal void UpdateOnlineStatus(bool isOnline)
        {
            this.IsOnline = isOnline;
            this.onlineStatusChangedSubject.OnNext(new ServerOnlineStatusChanged(isOnline));
        }
       
        protected internal void UpdatePlayers(Player[] currentPlayers)
        {
            var oldPlayers = this.Players;
            var playersJoined = currentPlayers.Except(oldPlayers).ToArray();
            var playersLeft = oldPlayers.Except(currentPlayers).ToArray();

            this.Players = currentPlayers;
            this.playersChangedSubject.OnNext(new ServerPlayersChanged(currentPlayers, playersJoined, playersLeft));
        }

        protected Subject<ServerOnlineStatusChanged> onlineStatusChangedSubject = new Subject<ServerOnlineStatusChanged>();
        protected Subject<ServerPlayersChanged> playersChangedSubject = new Subject<ServerPlayersChanged>();

        public virtual IObservable<ServerOnlineStatusChanged> OnlineStatusChanged => this.onlineStatusChangedSubject;
        public virtual IObservable<ServerPlayersChanged> PlayersChanged => this.playersChangedSubject;
    }
}