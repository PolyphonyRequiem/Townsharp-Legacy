namespace TownshipTale.Api.Identity
{
    public readonly record struct AccessToken(string Token = "", int ExpiresIn=Int32.MinValue, string TokenType="", string Scope = "")
    {
        public DateTime IssuedAt { get; } = DateTime.UtcNow;

        public DateTime ExpiresAt => IssuedAt.AddSeconds(this.ExpiresIn);

        public bool Expiring => DateTime.UtcNow > this.ExpiresAt.AddMinutes(-5);

        public bool Expired => DateTime.UtcNow > this.ExpiresAt;

        public static readonly AccessToken None = new AccessToken();

        public override string ToString()
        {
            return $"{TokenType} {Token}";
        }
    }
}