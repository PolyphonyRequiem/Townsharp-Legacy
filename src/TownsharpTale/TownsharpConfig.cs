namespace Townsharp
{
    public class TownsharpConfig
    {
        public TownsharpConfig()
        {

        }

        public int MaxDegreeOfParallelism { get; set; } = 4;

        public static TownsharpConfig Default { get; set; } = new TownsharpConfig();
    }
}