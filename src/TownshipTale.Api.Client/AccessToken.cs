namespace TownshipTale.Api.Client
{
    public readonly record struct AccessToken(string Token, int ExpiresIn, string TokenType, string[] Scope);
}