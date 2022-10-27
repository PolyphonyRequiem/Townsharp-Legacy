namespace TownshipTale.Api.Core
{
    public readonly record struct AccessToken(string Token = "", int ExpiresIn = int.MinValue, string TokenType = "", string Scope = "")
    {
        public DateTime IssuedAt { get; } = DateTime.UtcNow;

        public DateTime ExpiresAt => IssuedAt.AddSeconds(ExpiresIn);

        public bool Expiring => DateTime.UtcNow > ExpiresAt.AddMinutes(-5);

        public bool Expired => DateTime.UtcNow > ExpiresAt;

        public static readonly AccessToken None = new AccessToken();

        public override string ToString()
        {
            return $"{TokenType} {Token}";
        }
    }
}