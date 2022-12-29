namespace Townsharp.Servers
{
    public class Player
    {
        // NOTE:  needs to maintain entity referential integrity
        public Player(PlayerId id, string userName)
        {
            this.Id = id;
            this.UserName = userName;
        }

        public PlayerId Id { get; set; }    

        public string UserName { get; set; }
    }
}