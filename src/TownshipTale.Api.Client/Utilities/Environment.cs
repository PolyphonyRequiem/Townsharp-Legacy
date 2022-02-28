namespace TownshipTale.Api.Client
{
    internal static class Environment
    {
        internal static string GetRequiredEnvironmentVariable(string name)
            => System.Environment.GetEnvironmentVariable(name) 
            ?? throw new KeyNotFoundException($"The environment variable '{name}' was expected but not assigned.");
    }
}
