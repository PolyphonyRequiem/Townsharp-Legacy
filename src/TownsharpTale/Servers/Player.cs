namespace Townsharp.Servers
{
    public class Player
    {
        // NOTE:  needs to maintain entity referential integrity
        public Player(PlayerId id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public PlayerId Id { get; set; }    

        public string Name { get; set; }
    }
}