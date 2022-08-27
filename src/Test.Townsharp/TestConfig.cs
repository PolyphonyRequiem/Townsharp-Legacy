using Townsharp;
using Townsharp.Identity;

namespace Test.Townsharp
{
    internal class TestConfig
    {
        public static TownsharpConfig DefaultConfig { get; internal set; } = new TownsharpConfig(new IdentityConfig());
    }
}